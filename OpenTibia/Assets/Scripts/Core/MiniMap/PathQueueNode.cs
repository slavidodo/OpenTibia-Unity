using System;

namespace OpenTibiaUnity.Core.MiniMap
{
    class PathQueueNode : Priority_Queue.FastPriorityQueueNode
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int Cost { get; set; } = int.MaxValue;
        public int Distance { get; set; } = int.MaxValue;
        public int PathCost { get; set; } = int.MaxValue;
        public int PathHeuristic { get; set; } = int.MaxValue;
        public PathQueueNode Predecessor { get; set; } = null;

        public PathQueueNode(int x, int y) {
            this.x = x;
            this.y = y;
            Distance = Math.Abs(x) + Math.Abs(y);
        }
    }
}
