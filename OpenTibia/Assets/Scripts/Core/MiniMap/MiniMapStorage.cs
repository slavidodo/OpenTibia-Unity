using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.MiniMap
{
    public class MiniMapStorage
    {
        public class PositionChangeEvent : UnityEvent<MiniMapStorage, Vector3Int, Vector3Int> { }

        private bool _changedSector = false;
        private Vector3Int _sector = new Vector3Int(-1, -1, -1);
        private Vector3Int _position = new Vector3Int(-1, -1, -1);
        private Utils.Heap _pathHeap;
        private List<MiniMapSector> _sectorCache = new List<MiniMapSector>();
        private List<MiniMapSector> _loadQueue = new List<MiniMapSector>();
        private List<MiniMapSector> _saveQueue = new List<MiniMapSector>();
        private SortedList<int, PathItem> _pathMatrix;
        private List<PathItem> _pathDirty = new List<PathItem>();
        private int _currentIOCount = 0;

        public int PositionX { get { return _position.x; } }
        public int PositionY { get { return _position.y; } }
        public int PositionZ { get { return _position.z; } }

        public PositionChangeEvent onPositionChange = new PositionChangeEvent();

        public Vector3Int Position {
            get { return _position; }
            set {
                var oldPosition = _position;

                value.x = Mathf.Clamp(value.x, Constants.MapMinX, Constants.MapMaxX);
                value.y = Mathf.Clamp(value.y, Constants.MapMinY, Constants.MapMaxY);
                value.z = Mathf.Clamp(value.z, Constants.MapMinZ, Constants.MapMaxZ);

                int sectorX = (value.x / Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
                int sectorY = (value.y / Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
                _position = value;

                if (sectorX != _sector.x || sectorY != _sector.y || value.z != _sector.z) {
                    for (int i = -1; i < 2; i++) {
                        for (int j = -1; j < 2; j++) {
                            AcquireSector(
                                sectorX + j * Constants.MiniMapSectorSize,
                                sectorY + i * Constants.MiniMapSectorSize,
                                value.z,
                                i == 0 && j == 0);
                        }
                    }

                    _sector.Set(sectorX, sectorY, value.z);
                }

                onPositionChange.Invoke(this, Position, oldPosition);
            }
        }

        public MiniMapStorage() {
            _pathMatrix = new SortedList<int, PathItem>(Constants.PathMatrixCenter * Constants.PathMatrixCenter);
            for (int y = 0; y < Constants.PathMatrixSize; y++) {
                for (int x = 0; x < Constants.PathMatrixSize; x++)
                    _pathMatrix.Add(y * Constants.PathMatrixSize + x, new PathItem(x - Constants.PathMatrixCenter, y- Constants.PathMatrixCenter));
            }

            _pathHeap = new Utils.Heap(50000);
            _pathDirty = new List<PathItem>();
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
            int it = _sectorCache.Count - 1;
            while (it > 0) {
                var tmpSector = _sectorCache[it];
                if (tmpSector.Equals(sectorX, sectorY, sectorZ)) {
                    sector = tmpSector;
                    _sectorCache.RemoveAt(it);
                    break;
                }

                it--;
            }

            if (!sector) {
                it = _saveQueue.Count - 1;
                while (it >= 0) {
                    var tmpSector = _saveQueue[it];
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
                    Dequeue(_loadQueue, sector);
                }
            }

            if (_sectorCache.Count >= Constants.MiniMapSectorSize) {
                MiniMapSector tmpSector = null;
                foreach (var sec in _sectorCache) {
                    if (!sec.Dirty) {
                        tmpSector = sec;
                        _sectorCache.Remove(sec);
                        break;
                    }
                }

                if (!tmpSector) {
                    tmpSector = _sectorCache[0];
                    _sectorCache.RemoveAt(0);
                }

                Dequeue(_loadQueue, tmpSector);
                if (tmpSector.Dirty)
                    Enqueue(_saveQueue, tmpSector);
            }

            _sectorCache.Add(sector);
            return sector;
        }

        public void UpdateField(Vector3Int position, uint color, int cost, bool fireChangeCall) {
            var sector = AcquireSector(position, false);
            sector.UpdateField(position, color, cost);
            
            if (fireChangeCall) {
                // fire change.

                Enqueue(_saveQueue, sector);
            } else {
                _changedSector = true;
            }
        }

        public int GetFieldCost(Vector3Int absolutePosition) {
            return GetFieldCost(absolutePosition.x, absolutePosition.y, absolutePosition.z);
        }

        public int GetFieldCost(int x, int y, int z) {
            return AcquireSector(x, y, z, false).GetCost(x, y, z);
        }

        public void RefreshSectors() {
            if (_changedSector) {
                // TODO: dispatch change
                int it = _sectorCache.Count - 1;
                while (it >= 0) {
                    var sector = _sectorCache[it];
                    if (sector.Dirty) {
                        Enqueue(_saveQueue, sector);
                    }

                    it--;
                }
                _changedSector = false;
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
                return PathState.PathErrorpublic;
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
            PathItem pathItem = _pathMatrix[matrixCenter * matrixSize + matrixCenter];
            pathItem.Reset();
            pathItem.Predecessor = null;
            pathItem.Cost = int.MaxValue;
            pathItem.PathCost = int.MaxValue;
            pathItem.PathHeuristic = 0;
            _pathDirty.Add(pathItem); // push the initial position to the closed list

            // obtain the final position at our grid
            PathItem lastPathNode = _pathMatrix[(matrixCenter + dY) * Constants.PathMatrixSize + (matrixCenter + dX)];
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
            _pathDirty.Add(lastPathNode);

            // clear our heap and push the current node to it.
            _pathHeap.Clear(false);
            _pathHeap.AddItem(lastPathNode, lastPathNode.PathHeuristic);

            PathItem currentPathItem = null;
            PathItem tmpPathItem = null;
            
            // looping through the very first SQM in the heap
            while ((currentPathItem = _pathHeap.ExtractMinItem() as PathItem) != null) {
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

                                    tmpPathItem = _pathMatrix[(matrixCenter + gridY) * matrixSize + (matrixCenter + gridX)];
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
                                            _pathDirty.Add(tmpPathItem);
                                        }

                                        if (!(tmpPathItem == pathItem || tmpPathItem.Cost >= Constants.PathCostObstacle)) {
                                            if (tmpPathItem.HeapParent != null) {
                                                _pathHeap.UpdateKey(tmpPathItem, currentPathCost + tmpPathItem.PathHeuristic);
                                            } else {
                                                tmpPathItem.Reset();
                                                _pathHeap.AddItem(tmpPathItem, currentPathCost + tmpPathItem.PathHeuristic);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var ret = PathState.PathErrorpublic;
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

            foreach (var tmp in _pathDirty) {
                tmp.Cost = int.MaxValue;
                tmp.PathCost = int.MaxValue;
            }
            _pathDirty.Clear();
            return ret;
        }

        public void OnIOTimer() {
            _currentIOCount++;

            if (_loadQueue != null && _loadQueue.Count > 0 && _loadQueue[0] != null)
                _loadQueue[0].LoadSharedObject();
            else if (_currentIOCount % 10 == 0 && _saveQueue != null && _saveQueue.Count > 0 && _saveQueue[0] != null)
                _saveQueue[0].SaveSharedObject();
        }

        // used in export MiniMap
        public void SaveSectors() {
            foreach (var sector in _saveQueue)
                sector.SaveSharedObject();

            foreach (var sector in _sectorCache)
                sector.SaveSharedObject();
        }
    }
}
