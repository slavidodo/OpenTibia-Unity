using UnityEngine;

namespace OpenTibiaUnity.Core.MiniMap
{
    public class PathItem : Utils.HeapItem
    {
        public int X = 0;
        public int Y = 0;

        public int Cost = int.MaxValue;
        public int PathHeuristic = int.MaxValue;
        public int PathCost = int.MaxValue;
        public int Distance = int.MaxValue;

        public PathItem Predecessor = null;

        public PathItem(int x, int y) {
            X = x;
            Y = y;

            Distance = Mathf.Abs(x) + Mathf.Abs(y);
        }
    }
}
