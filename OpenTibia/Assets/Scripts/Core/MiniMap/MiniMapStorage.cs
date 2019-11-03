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
        private List<MiniMapSector> _sectorCache = new List<MiniMapSector>();
        private List<MiniMapSector> _loadQueue = new List<MiniMapSector>();
        private List<MiniMapSector> _saveQueue = new List<MiniMapSector>();
        private int _currentIOCount = 0;

        public int PositionX { get { return _position.x; } }
        public int PositionY { get { return _position.y; } }
        public int PositionZ { get { return _position.z; } }

        public PositionChangeEvent onPositionChange = new PositionChangeEvent();

        public Vector3Int Position {
            get { return _position; }
            set {
                var oldPosition = _position;
                if (oldPosition == value)
                    return;

                value.x = Mathf.Clamp(value.x, Constants.MapMinX, Constants.MapMaxX);
                value.y = Mathf.Clamp(value.y, Constants.MapMinY, Constants.MapMaxY);
                value.z = Mathf.Clamp(value.z, Constants.MapMinZ, Constants.MapMaxZ);

                int sectorX = (value.x / Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
                int sectorY = (value.y / Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
                _position = value;

                if (sectorX != _sector.x || sectorY != _sector.y || value.z != _sector.z) {
                    for (int j = -1; j <= 1; j++) {
                        for (int i = -1; i <= 1; i++) {
                            AcquireSector(
                                sectorX + i * Constants.MiniMapSectorSize,
                                sectorY + j * Constants.MiniMapSectorSize,
                                value.z,
                                j == 0 && i == 0);
                        }
                    }

                    _sector.Set(sectorX, sectorY, value.z);
                }

                onPositionChange.Invoke(this, _position, oldPosition);
            }
        }

        public MiniMapSector AcquireSector(Vector3Int position, bool cache) {
            return AcquireSector(position.x, position.y, position.z, cache);
        }

        public MiniMapSector AcquireSector(int x, int y, int z, bool forceCache) {
            x = System.Math.Max(Constants.MapMinX, System.Math.Min(x, Constants.MapMaxX));
            y = System.Math.Max(Constants.MapMinY, System.Math.Min(y, Constants.MapMaxY));
            z = System.Math.Max(Constants.MapMinZ, System.Math.Min(z, Constants.MapMaxZ));

            x = x / Constants.MiniMapSectorSize * Constants.MiniMapSectorSize;
            y = y / Constants.MiniMapSectorSize * Constants.MiniMapSectorSize;

            MiniMapSector sector = _sectorCache.Find((sec) => sec.Equals(x, y, z));
            if (sector)
                return sector;
            else

            if (!sector) {
                sector = new MiniMapSector(x, y, z);
                if (forceCache) {
                    // load it right now
                    sector.LoadSharedObject();

                    // if it exists in the load queue, then remove it
                    Dequeue(_loadQueue, sector);
                } else {
                    // don't load cache, it ain't really needed
                    Enqueue(_loadQueue, sector);
                }
            }

            // pop the oldest sector
            if (_sectorCache.Count >= Constants.MiniMapCacheSize)
                _sectorCache.RemoveAt(0);

            _sectorCache.Add(sector);
            return sector;
        }

        public void UpdateField(Vector3Int position, uint color, int cost, bool fireChangeCall) {
            var sector = AcquireSector(position, true);
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
                    if (sector.Dirty)
                        Enqueue(_saveQueue, sector);

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
            return CalculateAStartPath(startX, startY, startZ, targetX, targetY, targetZ, 50000, steps);
        }

        private PathState CalculateAStartPath(int startX, int startY, int startZ, int targetX, int targetY, int targetZ, int searchLimit, List<int> steps) {
            MiniMapSector[] sectors = new MiniMapSector[4];
            sectors[0] = AcquireSector(startX - Constants.PathMatrixCenter, startY - Constants.PathMatrixCenter, startZ, false);
            sectors[1] = AcquireSector(startX - Constants.PathMatrixCenter, startY + Constants.PathMatrixCenter, startZ, false);
            sectors[2] = AcquireSector(startX + Constants.PathMatrixCenter, startY + Constants.PathMatrixCenter, startZ, false);
            sectors[3] = AcquireSector(startX + Constants.PathMatrixCenter, startY - Constants.PathMatrixCenter, startZ, false);

            // initial sector position (start position in minimap storage)
            var sectorMaxX = sectors[0].SectorX + Constants.MiniMapSectorSize;
            var sextorMaxY = sectors[0].SectorY + Constants.MiniMapSectorSize;

            var closedList = new Dictionary<int, PathItem>();
            var openList = new Utils.PriorityQueue<KeyValuePair<PathItem, int>>(Comparer<KeyValuePair<PathItem, int>>.Create(ComparePathNodes));

            PathItem currentNode = new PathItem(startX, startY);
            closedList.Add(Hash2DPosition(startX, startY), currentNode);

            PathItem foundNode = null;

            PathState ret = PathState.PathErrorInternal;
            while (currentNode != null) {
                if (closedList.Count > searchLimit) {
                    ret = PathState.PathErrorTooFar;
                    break;
                }

                if (currentNode.X == targetX && currentNode.Y == targetY && (foundNode == null || currentNode.PathCost < foundNode.PathCost))
                    foundNode = currentNode;

                if (foundNode != null && (currentNode.PathHeuristic >= foundNode.PathCost))
                    break;

                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        if (i == 0 && j == 0)
                            continue;

                        int currentPosX = currentNode.X + i;
                        int currentPosY = currentNode.Y + j;
                        int sectorIndex;
                        if (currentPosX < sectorMaxX)
                            sectorIndex = currentPosY < sextorMaxY ? 0 : 1;
                        else
                            sectorIndex = currentPosY < sextorMaxY ? 3 : 2;

                        int cost = sectors[sectorIndex].GetCost(currentPosX, currentPosY, startZ);
                        if (cost >= Constants.PathCostObstacle)
                            continue;

                        int modifier = 1;
                        if ((i * j) != 0)
                            modifier = 3;

                        int pathCost = currentNode.PathCost + modifier * cost;
                        var direction = DirectionFromPosToPos(currentNode.X, currentNode.Y, currentPosX, currentPosY);

                        PathItem neighborNode;
                        if (closedList.TryGetValue(Hash2DPosition(currentPosX, currentPosY), out PathItem handledNode)) {
                            neighborNode = handledNode;
                            if (neighborNode.PathCost <= pathCost)
                                continue;
                        } else {
                            neighborNode = new PathItem(currentPosX, currentPosY);
                            closedList.Add(Hash2DPosition(currentPosX, currentPosY), neighborNode);
                        }

                        neighborNode.Predecessor = currentNode;
                        neighborNode.Cost = cost;
                        neighborNode.PathCost = pathCost;
                        neighborNode.PathHeuristic = neighborNode.PathCost + Distance(currentPosX, currentPosY, targetX, targetY);
                        neighborNode.Direction = direction;

                        openList.Push(new KeyValuePair<PathItem, int>(neighborNode, neighborNode.PathHeuristic));
                    }
                }

                if (openList.Count > 0)
                    currentNode = openList.Pop().Key;
                else
                    currentNode = null;
            }

            if (foundNode != null) {
                currentNode = foundNode;
                while (currentNode != null) {
                    steps.Add((int)currentNode.Direction | currentNode.Cost << 16);
                    if (steps.Count + 1 >= Constants.PathMaxSteps)
                        break;
                    currentNode = currentNode.Predecessor;
                }

                steps.RemoveAt(steps.Count - 1);
                steps.Reverse();
                ret = PathState.PathExists;
            }

            return ret;
        }

        private int Hash2DPosition(int x, int y) {
            return x * 8192 + y;
        }

        private int Distance(int startX, int startY, int targetX, int targetY) {
            float d = Mathf.Sqrt(Mathf.Pow(targetX - startX, 2) + Mathf.Pow(targetY - startY, 2)) * 100;
            return (int)d;
        }

        private int ComparePathNodes(KeyValuePair<PathItem, int> a, KeyValuePair<PathItem, int> b) {
            return a.Value.CompareTo(b.Value);
        }

        private PathDirection DirectionFromPosToPos(int startX, int startY, int targetX, int targetY) {
            int dX = Mathf.Clamp(targetX - startX, -1, 1);
            int dY = Mathf.Clamp(targetY - startY, -1, 1);
            if (dX >= 1 && dY == 0)
                return PathDirection.East;
            else if (dX == 1 && dY == -1)
                return PathDirection.NorthEast;
            else if (dX == 0 && dY == -1)
                return PathDirection.North;
            else if (dX == -1 && dY == -1)
                return PathDirection.NorthWest;
            else if (dX == -1 && dY == 0)
                return PathDirection.West;
            else if (dX == -1 && dY == 1)
                return PathDirection.SouthWest;
            else if (dX == 0 && dY == 1)
                return PathDirection.South;
            else if (dX == 1 && dY == 1)
                return PathDirection.SouthEast;
            return PathDirection.Invalid;
        }
        public void OnIOTimer() {
            _currentIOCount++;

            if (_loadQueue.Count > 0) {
                _loadQueue[0].LoadSharedObject();
                _loadQueue.RemoveAt(0);
            } else if (_currentIOCount % 10 == 0 && _saveQueue.Count > 0) {
                _saveQueue[0].SaveSharedObject();
                _saveQueue.RemoveAt(0);
            }
        }

        public void SaveSectors() {
            foreach (var sector in _saveQueue)
                sector.SaveSharedObject();
            _saveQueue.Clear();

            foreach (var sector in _sectorCache) {
                if (sector.Dirty)
                    sector.SaveSharedObject();
            }
        }
    }
}
