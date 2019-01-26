using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.MiniMap
{
    public class MiniMapStorage
    {
        public class PositionChangeEvent : UnityEvent<MiniMapStorage, Vector3Int, Vector3Int> { }

        private bool m_ChangedSector = false;
        private Vector3Int m_Sector = new Vector3Int(-1, -1, -1);
        private Vector3Int m_Position = new Vector3Int(-1, -1, -1);
        private Priority_Queue.FastPriorityQueue<PathQueueNode> m_PathPriorityQueue;
        private List<MiniMapSector> m_SectorCache = new List<MiniMapSector>();
        private List<MiniMapSector> m_LoadQueue = new List<MiniMapSector>();
        private List<MiniMapSector> m_SaveQueue = new List<MiniMapSector>();
        private SortedList<int, PathQueueNode> m_PathMatrix;
        private List<PathQueueNode> m_PathDirty = new List<PathQueueNode>();
        private int m_CurrentIOCount = 0;

        public int PositionX { get { return m_Position.x; } }
        public int PositionY { get { return m_Position.y; } }
        public int PositionZ { get { return m_Position.z; } }

        public PositionChangeEvent onPositionChange = new PositionChangeEvent();

        public Vector3Int Position {
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

        public MiniMapStorage() {
            m_PathMatrix = new SortedList<int, PathQueueNode>(Constants.PathMatrixCenter * Constants.PathMatrixCenter);
            for (int y = 0; y < Constants.PathMatrixSize; y++) {
                for (int x = 0; x < Constants.PathMatrixSize; x++) {
                    m_PathMatrix.Add(y * Constants.PathMatrixSize + x, new PathQueueNode(x - Constants.PathMatrixCenter, y- Constants.PathMatrixCenter));
                }
            }
        }

        public MiniMapSector AcquireSector(Vector3Int position, bool cache) {
            return AcquireSector(position.x, position.y, position.z, cache);
        }

        public MiniMapSector AcquireSector(int x, int y, int z, bool cache) {
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

        public void UpdateField(Vector3Int position, uint color, int cost, bool fireChangeCall) {
            var sector = AcquireSector(position, false);
            sector.UpdateField(position, color, cost);
            
            if (fireChangeCall) {
                // fire change.

                Enqueue(m_SaveQueue, sector);
            } else {
                m_ChangedSector = true;
            }
        }

        public int GetFieldCost(Vector3Int absolutePosition) {
            return GetFieldCost(absolutePosition.x, absolutePosition.y, absolutePosition.z);
        }

        public int GetFieldCost(int x, int y, int z) {
            return AcquireSector(x, y, z, false).GetCost(x, y, z);
        }

        public void RefreshSectors() {
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

        public void Enqueue(List<MiniMapSector> queue, MiniMapSector sector) {
            if (!queue.Find((sec) => sector.Equals(sec)))
                queue.Add(sector);
        }

        public void Dequeue(List<MiniMapSector> queue, MiniMapSector sector) {
            queue.Remove(sector);
        }

        public PathState CalculatePath(Vector3Int start, Vector3Int target, bool diagonal, bool exact, List<int> steps) {
            return CalculatePath(start.x, start.y, start.z, target.x, target.y, target.z, diagonal, exact, steps);
        }

        public PathState CalculatePath(int startX, int startY, int startZ, int targetX, int targetY, int targetZ, bool forceDiagonal, bool forceExact, List<int> steps) {
            int dX = targetX - startX;
            int dY = targetY - startY;
            if (steps == null)
                return PathState.PathErrorInternal;
            else if (targetZ > startZ)
                return PathState.PathErrorGoUpstairs;
            else if (targetZ < startZ)
                return PathState.PathErrorGoDownstairs;
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
            var overallMinCost = int.MaxValue;
            foreach (var sector in tmpSectors)
                overallMinCost = System.Math.Min(overallMinCost, sector.MinCost);

            // initial sector position (start position in minimap storage)
            var sectorMaxX = tmpSectors[0].SectorX + Constants.MiniMapSectorSize;
            var sextorMaxY = tmpSectors[0].SectorY + Constants.MiniMapSectorSize;

            // obtain the center of the grid, and resetting it
            // the center of the grid is matchin our initial position, so we will use MatrixCenter with offset as a workaround
            PathQueueNode pathNode = m_PathMatrix[matrixCenter * matrixSize + matrixCenter];
            pathNode.Reset();
            pathNode.Predecessor = null;
            pathNode.Cost = pathNode.PathCost = int.MaxValue;
            pathNode.PathHeuristic = 0;
            
            m_PathDirty.Add(pathNode); // push the initial position to the closed list

            // obtain the final position at our grid
            PathQueueNode lastPathNode = m_PathMatrix[(matrixCenter + dY) * Constants.PathMatrixSize + (matrixCenter + dX)];
            lastPathNode.Predecessor = null;
            lastPathNode.Reset();

            int tmpIndex;
            if (targetX < sectorMaxX) {
                if (targetY < sextorMaxY) {
                    tmpIndex = (int)Directions.North;
                } else {
                    tmpIndex = (int)Directions.East;
                }
            } else if (targetY < sextorMaxY) {
                tmpIndex = (int)Directions.West;
            } else {
                tmpIndex = (int)Directions.South;
            }
            
            lastPathNode.Cost = tmpSectors[tmpIndex].GetCost(targetX, targetY, targetZ);
            lastPathNode.PathCost = 0;
            // from the constructor, the distance is the manhattan distance from start_pos to target_pos
            lastPathNode.PathHeuristic = lastPathNode.Cost + (lastPathNode.Distance - 1) * overallMinCost;

            // now add that to our closed list
            m_PathDirty.Add(lastPathNode);

            // clear our heap and push the current node to it.
            m_PathPriorityQueue = new Priority_Queue.FastPriorityQueue<PathQueueNode>(50000);
            m_PathPriorityQueue.Enqueue(lastPathNode, lastPathNode.PathHeuristic);

            PathQueueNode currentPathNode = null;
            PathQueueNode tmpPathNode = null;

            uint s = 0, s2 = 0;
            // looping through the very first SQM in the heap
            while (m_PathPriorityQueue.Count > 0) {
                s++;
                currentPathNode = m_PathPriorityQueue.Dequeue();
                // check if the current move won't exceed our current shortest path, otherwise end it up
                // if it exceeds, then we will loop again through our heap, if exists
                // if not, then we are done searching if the current path is undefined that means we can't
                // reach that field
                if (currentPathNode.Priority >= pathNode.PathCost)
                    break;

                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        if (i == 0 && j == 0)
                            continue;

                        int gridX = currentPathNode.x + i;
                        int gridY = currentPathNode.y + j;

                        // check if that grid is in the range or validity
                        if (gridX < -matrixCenter || gridX > matrixCenter || gridY < -matrixCenter || gridY > matrixCenter)
                            continue;
                        
                        int currentPathCost;
                        if (i * j == 0) // straight movement (not diagonal)
                            currentPathCost = currentPathNode.PathCost + currentPathNode.Cost;
                        else // diagonal movements worth as 3 as a normal movement;
                            currentPathCost = currentPathNode.PathCost + 3 * currentPathNode.Cost;

                        tmpPathNode = m_PathMatrix[(matrixCenter + gridY) * matrixSize + (matrixCenter + gridX)];
                        if (tmpPathNode.PathCost > currentPathCost) {
                            tmpPathNode.Predecessor = currentPathNode;
                            tmpPathNode.PathCost = currentPathCost;
                            if (tmpPathNode.Cost == int.MaxValue) {
                                int currentPosX = startX + tmpPathNode.x;
                                int currentPosY = startY + tmpPathNode.y;
                                if (currentPosX < sectorMaxX) {
                                    if (currentPosY < sextorMaxY)
                                        tmpIndex = (int)Directions.North;
                                    else
                                        tmpIndex = (int)Directions.East;
                                } else if (currentPosY < sextorMaxY) {
                                    tmpIndex = (int)Directions.West;
                                } else {
                                    tmpIndex = (int)Directions.South;
                                }
                                tmpPathNode.Cost = tmpSectors[tmpIndex].GetCost(currentPosX, currentPosY, startZ);
                                tmpPathNode.PathHeuristic = tmpPathNode.Cost + (tmpPathNode.Distance - 1) * overallMinCost;
                                m_PathDirty.Add(tmpPathNode);
                            }

                            if (tmpPathNode == pathNode || tmpPathNode.Cost >= Constants.PathCostObstacle)
                                continue;

                            s2++;
                            if (tmpPathNode.Queue == m_PathPriorityQueue) {
                                m_PathPriorityQueue.UpdatePriority(tmpPathNode, currentPathCost + tmpPathNode.PathHeuristic);
                            } else {
                                tmpPathNode.Reset();
                                m_PathPriorityQueue.Enqueue(tmpPathNode, currentPathCost + tmpPathNode.PathHeuristic);
                            }
                        }
                    }
                }
            }

            //Debug.Log("Recuresive Count: " + s + ", " + s2);
            
            var ret = PathState.PathErrorInternal;
            if (pathNode.PathCost < int.MaxValue) {
                currentPathNode = pathNode;
                tmpPathNode = null;
                while (currentPathNode != null) {
                    if (!forceExact && currentPathNode.x == lastPathNode.x && currentPathNode.y == lastPathNode.y && lastPathNode.Cost >= Constants.PathCostObstacle) {
                        currentPathNode = null;
                        break;
                    }
                    
                    if (currentPathNode.Cost == Constants.PathCostUndefined)
                        break;

                    if (tmpPathNode != null) {
                        dX = currentPathNode.x - tmpPathNode.x;
                        dY = currentPathNode.y - tmpPathNode.y;
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
                        steps[steps.Count - 1] = steps[steps.Count - 1] | currentPathNode.Cost << 16;
                        if (steps.Count + 1 >= Constants.PathMaxSteps) {
                            break;
                        }
                    }

                    tmpPathNode = currentPathNode;
                    currentPathNode = currentPathNode.Predecessor;
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

        public void OnIOTimer() {
            m_CurrentIOCount++;

            if (m_LoadQueue != null && m_LoadQueue.Count > 0 && m_LoadQueue[0] != null)
                m_LoadQueue[0].LoadSharedObject();
            else if (m_CurrentIOCount % 10 == 0 && m_SaveQueue != null && m_SaveQueue.Count > 0 && m_SaveQueue[0] != null)
                m_SaveQueue[0].SaveSharedObject();
        }

        // used in export MiniMap
        public void SaveSectors() {
            foreach (var sector in m_SaveQueue)
                sector.SaveSharedObject();

            foreach (var sector in m_SectorCache)
                sector.SaveSharedObject();
        }
    }
}
