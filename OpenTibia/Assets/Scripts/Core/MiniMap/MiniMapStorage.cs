using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.MiniMap
{
    internal class MiniMapStorage
    {
        internal class PositionChangeEvent : UnityEvent<MiniMapStorage, Vector3Int, Vector3Int> { }

        private bool m_ChangedSector = false;
        private Vector3Int m_Sector = new Vector3Int(-1, -1, -1);
        private Vector3Int m_Position = new Vector3Int(-1, -1, -1);
        private Utility.Heap m_PathHeap;
        private List<MiniMapSector> m_SectorCache = new List<MiniMapSector>();
        private List<MiniMapSector> m_LoadQueue = new List<MiniMapSector>();
        private List<MiniMapSector> m_SaveQueue = new List<MiniMapSector>();
        private SortedList<int, PathItem> m_PathMatrix;
        private List<PathItem> m_PathDirty = new List<PathItem>();
        private int m_CurrentIOCount = 0;

        internal int PositionX { get { return m_Position.x; } }
        internal int PositionY { get { return m_Position.y; } }
        internal int PositionZ { get { return m_Position.z; } }

        internal PositionChangeEvent onPositionChange = new PositionChangeEvent();

        internal Vector3Int Position {
            get { return m_Position; }
            set {
                var oldPosition = m_Position;

                value.x = Mathf.Clamp(value.x, Constants.MapMinX, Constants.MapMaxX);
                value.y = Mathf.Clamp(value.y, Constants.MapMinY, Constants.MapMaxY);
                value.z = Mathf.Clamp(value.z, Constants.MapMinZ, Constants.MapMaxZ);

                int sectorX = (value.x / Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
                int sectorY = (value.y / Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
                m_Position = value;

                if (sectorX != m_Sector.x || sectorY != m_Sector.y || value.z != m_Sector.z) {
                    for (int i = -1; i < 2; i++) {
                        for (int j = -1; j < 2; j++) {
                            AcquireSector(
                                sectorX + j * Constants.MiniMapSectorSize,
                                sectorY + i * Constants.MiniMapSectorSize,
                                value.z,
                                i == 0 && j == 0);
                        }
                    }

                    m_Sector.Set(sectorX, sectorY, value.z);
                }

                onPositionChange.Invoke(this, Position, oldPosition);
            }
        }

        internal MiniMapStorage() {
            m_PathMatrix = new SortedList<int, PathItem>(Constants.PathMatrixCenter * Constants.PathMatrixCenter);
            for (int y = 0; y < Constants.PathMatrixSize; y++) {
                for (int x = 0; x < Constants.PathMatrixSize; x++)
                    m_PathMatrix.Add(y * Constants.PathMatrixSize + x, new PathItem(x - Constants.PathMatrixCenter, y- Constants.PathMatrixCenter));
            }

            m_PathHeap = new Utility.Heap(50000);
            m_PathDirty = new List<PathItem>();
        }

        internal MiniMapSector AcquireSector(Vector3Int position, bool cache) {
            return AcquireSector(position.x, position.y, position.z, cache);
        }

        internal MiniMapSector AcquireSector(int x, int y, int z, bool cache) {
            x = System.Math.Max(Constants.MapMinX, System.Math.Min(x, Constants.MapMaxX));
            y = System.Math.Max(Constants.MapMinY, System.Math.Min(y, Constants.MapMaxY));
            z = System.Math.Max(Constants.MapMinZ, System.Math.Min(z, Constants.MapMaxZ));

            int sectorX = x / Constants.MiniMapSectorSize * Constants.MiniMapSectorSize;
            int sectorY = y / Constants.MiniMapSectorSize * Constants.MiniMapSectorSize;
            int sectorZ = z;

            MiniMapSector sector = null;
            int it = m_SectorCache.Count - 1;
            while (it > 0) {
                var tmpSector = m_SectorCache[it];
                if (tmpSector.Equals(sectorX, sectorY, sectorZ)) {
                    sector = tmpSector;
                    m_SectorCache.RemoveAt(it);
                    break;
                }

                it--;
            }

            if (!sector) {
                it = m_SaveQueue.Count - 1;
                while (it >= 0) {
                    var tmpSector = m_SaveQueue[it];
                    if (tmpSector.Equals(sectorX, sectorY, sectorZ)) {
                        sector = tmpSector;
                        break;
                    }

                    it--;
                }
            }

            if (!sector) {
                sector = new MiniMapSector(sectorX, sectorY, sectorZ);
                if (cache) {
                    sector.LoadSharedObject();
                    Dequeue(m_LoadQueue, sector);
                }
            }

            if (m_SectorCache.Count >= Constants.MiniMapSectorSize) {
                MiniMapSector tmpSector = null;
                foreach (var sec in m_SectorCache) {
                    if (!sec.Dirty) {
                        tmpSector = sec;
                        m_SectorCache.Remove(sec);
                        break;
                    }
                }

                if (!tmpSector) {
                    tmpSector = m_SectorCache[0];
                    m_SectorCache.RemoveAt(0);
                }

                Dequeue(m_LoadQueue, tmpSector);
                if (tmpSector.Dirty)
                    Enqueue(m_SaveQueue, tmpSector);
            }

            m_SectorCache.Add(sector);
            return sector;
        }

        internal void UpdateField(Vector3Int position, uint color, int cost, bool fireChangeCall) {
            var sector = AcquireSector(position, false);
            sector.UpdateField(position, color, cost);
            
            if (fireChangeCall) {
                // fire change.

                Enqueue(m_SaveQueue, sector);
            } else {
                m_ChangedSector = true;
            }
        }

        internal int GetFieldCost(Vector3Int absolutePosition) {
            return GetFieldCost(absolutePosition.x, absolutePosition.y, absolutePosition.z);
        }

        internal int GetFieldCost(int x, int y, int z) {
            return AcquireSector(x, y, z, false).GetCost(x, y, z);
        }

        internal void RefreshSectors() {
            if (m_ChangedSector) {
                // TODO: dispatch change
                int it = m_SectorCache.Count - 1;
                while (it >= 0) {
                    var sector = m_SectorCache[it];
                    if (sector.Dirty) {
                        Enqueue(m_SaveQueue, sector);
                    }

                    it--;
                }
                m_ChangedSector = false;
            }
        }

        internal void Enqueue(List<MiniMapSector> queue, MiniMapSector sector) {
            if (!queue.Find((sec) => sector.Equals(sec)))
                queue.Add(sector);
        }

        internal void Dequeue(List<MiniMapSector> queue, MiniMapSector sector) {
            queue.Remove(sector);
        }

        internal PathState CalculatePath(Vector3Int start, Vector3Int target, bool diagonal, bool exact, List<int> steps) {
            return CalculatePath(start.x, start.y, start.z, target.x, target.y, target.z, diagonal, exact, steps);
        }

        internal PathState CalculatePath(int startX, int startY, int startZ, int targetX, int targetY, int targetZ, bool forceDiagonal, bool forceExact, List<int> steps) {
            int dX = targetX - startX;
            int dY = targetY - startY;
            if (steps == null)
                return PathState.PathErrorInternal;
            else if (targetZ > startZ)
                return PathState.PathErrorGoDownstairs;
            else if (targetZ < startZ)
                return PathState.PathErrorGoUpstairs;
            else if (dX == 0 && dY == 0)
                return PathState.PathEmpty;
            else if (System.Math.Abs(dX) >= Constants.PathMaxDistance || System.Math.Abs(dY) > Constants.PathMaxDistance)
                return PathState.PathErrorTooFar;

            // check if this tile is known to be obstacle
            
            // simple case, adjacent square
            if (System.Math.Abs(dX) + System.Math.Abs(dY) == 1) {
                int cost = GetFieldCost(targetX, targetY, targetZ);
                if (forceExact || cost < Constants.PathCostObstacle) {
                    if (dX == 1 && dY == 0)
                        steps.Add((int)PathDirection.East);
                    else if (dX == 0 && dY == -1)
                        steps.Add((int)PathDirection.North);
                    else if (dX == -1 && dY == 0)
                        steps.Add((int)PathDirection.West);
                    else if (dX == 0 && dY == 1)
                        steps.Add((int)PathDirection.South);

                    steps[0] = steps[0] | cost << 16;
                    return PathState.PathExists;
                }
                return PathState.PathEmpty;
            }

            // simple case, adjacent diagonal square (while diagonal is actually allowed)
            if (forceDiagonal && System.Math.Abs(dX) == 1 && System.Math.Abs(dY) == 1) {
                int cost = GetFieldCost(targetX, targetY, targetZ);
                if (forceExact || cost < Constants.PathCostObstacle) {
                    if (dX == 1 && dY == -1)
                        steps.Add((int)PathDirection.NorthEast);
                    else if (dX == -1 && dY == -1)
                        steps.Add((int)PathDirection.NorthWest);
                    else if (dX == -1 && dY == 1)
                        steps.Add((int)PathDirection.SouthWest);
                    else if (dX == 1 && dY == 1)
                        steps.Add((int)PathDirection.SouthEast);
                    steps[0] = steps[0] | cost << 16;
                    return PathState.PathExists;
                }
                return PathState.PathEmpty;
            }
            
            // A* Algorithm

            // acquiring 4 directional sectors
            MiniMapSector[] tmpSectors = new MiniMapSector[4];
            tmpSectors[0] = AcquireSector(startX - Constants.PathMatrixCenter, startY - Constants.PathMatrixCenter, startZ, false);
            tmpSectors[1] = AcquireSector(startX - Constants.PathMatrixCenter, startY + Constants.PathMatrixCenter, startZ, false);
            tmpSectors[2] = AcquireSector(startX + Constants.PathMatrixCenter, startY + Constants.PathMatrixCenter, startZ, false);
            tmpSectors[3] = AcquireSector(startX + Constants.PathMatrixCenter, startY - Constants.PathMatrixCenter, startZ, false);

            // obtain local variables of constants
            int matrixCenter = Constants.PathMatrixCenter;
            int matrixSize = Constants.PathMatrixSize;

            // heuristic multiplier
            var minCost = int.MaxValue;
            foreach (var sector in tmpSectors)
                minCost = System.Math.Min(minCost, sector.MinCost);

            // initial sector position (start position in minimap storage)
            var sectorMaxX = tmpSectors[0].SectorX + Constants.MiniMapSectorSize;
            var sextorMaxY = tmpSectors[0].SectorY + Constants.MiniMapSectorSize;

            // obtain the center of the grid, and resetting it
            // the center of the grid is matchin our initial position, so we will use MatrixCenter with offset as a workaround
            PathItem pathItem = m_PathMatrix[matrixCenter * matrixSize + matrixCenter];
            pathItem.Reset();
            pathItem.Predecessor = null;
            pathItem.Cost = int.MaxValue;
            pathItem.PathCost = int.MaxValue;
            pathItem.PathHeuristic = 0;
            m_PathDirty.Add(pathItem); // push the initial position to the closed list

            // obtain the final position at our grid
            PathItem lastPathNode = m_PathMatrix[(matrixCenter + dY) * Constants.PathMatrixSize + (matrixCenter + dX)];
            lastPathNode.Predecessor = null;
            lastPathNode.Reset();

            int tmpIndex;
            if (targetX < sectorMaxX)
                tmpIndex = targetY < sextorMaxY ? 0 : 1;
            else
                tmpIndex = targetY < sextorMaxY ? 3 : 2;
            
            lastPathNode.Cost = tmpSectors[tmpIndex].GetCost(targetX, targetY, targetZ);
            lastPathNode.PathCost = 0;
            // from the constructor, the distance is the manhattan distance from start_pos to target_pos
            lastPathNode.PathHeuristic = lastPathNode.Cost + (lastPathNode.Distance - 1) * minCost;

            // now add that to our closed list
            m_PathDirty.Add(lastPathNode);

            // clear our heap and push the current node to it.
            m_PathHeap.Clear(false);
            m_PathHeap.AddItem(lastPathNode, lastPathNode.PathHeuristic);

            PathItem currentPathItem = null;
            PathItem tmpPathItem = null;
            
            // looping through the very first SQM in the heap
            while ((currentPathItem = m_PathHeap.ExtractMinItem() as PathItem) != null) {
                // check if the current move won't exceed our current shortest path, otherwise end it up
                // if it exceeds, then we will loop again through our heap, if exists
                // if not, then we are done searching if the current path is undefined that means we can't
                // reach that field
                
                if (currentPathItem.HeapKey < pathItem.PathCost) {
                    for (int i = -1; i <= 1; i++) {
                        for (int j = -1; j <= 1; j++) {
                            if (i != 0 || j != 0) {
                                int gridX = currentPathItem.X + i;
                                int gridY = currentPathItem.Y + j;

                                // check if that grid is in the range or validity
                                if (!(gridX < -matrixCenter || gridX > matrixCenter || gridY < -matrixCenter || gridY > matrixCenter)) {
                                    int currentPathCost;
                                    if (i * j == 0) // straight movement (not diagonal)
                                        currentPathCost = currentPathItem.PathCost + currentPathItem.Cost;
                                    else // diagonal movements worth as 3 as a normal movement;
                                        currentPathCost = currentPathItem.PathCost + 3 * currentPathItem.Cost;

                                    tmpPathItem = m_PathMatrix[(matrixCenter + gridY) * matrixSize + (matrixCenter + gridX)];
                                    if (tmpPathItem.PathCost > currentPathCost) {
                                        tmpPathItem.Predecessor = currentPathItem;
                                        tmpPathItem.PathCost = currentPathCost;
                                        if (tmpPathItem.Cost == int.MaxValue) {
                                            int currentPosX = startX + tmpPathItem.X;
                                            int currentPosY = startY + tmpPathItem.Y;
                                            if (currentPosX < sectorMaxX)
                                                tmpIndex = currentPosY < sextorMaxY ? 0 : 1;
                                            else
                                                tmpIndex = currentPosY < sextorMaxY ? 3 : 2;

                                            tmpPathItem.Cost = tmpSectors[tmpIndex].GetCost(currentPosX, currentPosY, startZ);
                                            tmpPathItem.PathHeuristic = tmpPathItem.Cost + (tmpPathItem.Distance - 1) * minCost;
                                            m_PathDirty.Add(tmpPathItem);
                                        }

                                        if (!(tmpPathItem == pathItem || tmpPathItem.Cost >= Constants.PathCostObstacle)) {
                                            if (tmpPathItem.HeapParent != null) {
                                                m_PathHeap.UpdateKey(tmpPathItem, currentPathCost + tmpPathItem.PathHeuristic);
                                            } else {
                                                tmpPathItem.Reset();
                                                m_PathHeap.AddItem(tmpPathItem, currentPathCost + tmpPathItem.PathHeuristic);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var ret = PathState.PathErrorInternal;
            if (pathItem.PathCost < int.MaxValue) {
                currentPathItem = pathItem;
                tmpPathItem = null;
                while (currentPathItem != null) {
                    if (!forceExact && currentPathItem.X == lastPathNode.X && currentPathItem.Y == lastPathNode.Y && lastPathNode.Cost >= Constants.PathCostObstacle) {
                        currentPathItem = null;
                        break;
                    }
                    
                    if (currentPathItem.Cost == Constants.PathCostUndefined)
                        break;

                    if (tmpPathItem != null) {
                        dX = currentPathItem.X - tmpPathItem.X;
                        dY = currentPathItem.Y - tmpPathItem.Y;
                        if (dX == 1 && dY == 0) {
                            steps.Add((int)PathDirection.East);
                        } else if (dX == 1 && dY == -1) {
                            steps.Add((int)PathDirection.NorthEast);
                        } else if (dX == 0 && dY == -1) {
                            steps.Add((int)PathDirection.North);
                        } else if (dX == -1 && dY == -1) {
                            steps.Add((int)PathDirection.NorthWest);
                        } else if (dX == -1 && dY == 0) {
                            steps.Add((int)PathDirection.West);
                        } else if (dX == -1 && dY == 1) {
                            steps.Add((int)PathDirection.SouthWest);
                        } else if (dX == 0 && dY == 1) {
                            steps.Add((int)PathDirection.South);
                        } else if (dX == 1 && dY == 1) {
                            steps.Add((int)PathDirection.SouthEast);
                        }
                        steps[steps.Count - 1] = steps[steps.Count - 1] | currentPathItem.Cost << 16;
                        if (steps.Count + 1 >= Constants.PathMaxSteps) {
                            break;
                        }
                    }

                    tmpPathItem = currentPathItem;
                    currentPathItem = currentPathItem.Predecessor;
                }

                if (steps.Count == 0) {
                    ret = PathState.PathEmpty;
                } else {
                    ret = PathState.PathExists;
                }
            } else {
                ret = PathState.PathErrorUnreachable;
            }

            foreach (var tmp in m_PathDirty) {
                tmp.Cost = int.MaxValue;
                tmp.PathCost = int.MaxValue;
            }
            m_PathDirty.Clear();
            return ret;
        }

        internal void OnIOTimer() {
            m_CurrentIOCount++;

            if (m_LoadQueue != null && m_LoadQueue.Count > 0 && m_LoadQueue[0] != null)
                m_LoadQueue[0].LoadSharedObject();
            else if (m_CurrentIOCount % 10 == 0 && m_SaveQueue != null && m_SaveQueue.Count > 0 && m_SaveQueue[0] != null)
                m_SaveQueue[0].SaveSharedObject();
        }

        // used in export MiniMap
        internal void SaveSectors() {
            foreach (var sector in m_SaveQueue)
                sector.SaveSharedObject();

            foreach (var sector in m_SectorCache)
                sector.SaveSharedObject();
        }
    }
}
