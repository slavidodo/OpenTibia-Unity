using UnityEngine;

namespace OpenTibiaUnity.Core.MiniMap
{
    public sealed class PathItem
    {
        public int X = 0;
        public int Y = 0;

        public int Cost = 0;
        public int PathHeuristic = 0;
        public int PathCost = 0;
        public PathDirection Direction = PathDirection.Invalid;

        public PathItem Predecessor = null;

        public PathItem(int x, int y) {
            X = x;
            Y = y;
        }
    }
}
