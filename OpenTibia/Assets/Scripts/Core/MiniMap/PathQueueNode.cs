using System;

namespace OpenTibiaUnity.Core.MiniMap
{
    class PathQueueNode : Priority_Queue.FastPriorityQueueNode
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Cost { get; set; } = int.MaxValue;
        public int Distance { get; set; } = int.MaxValue;
        public int PathCost { get; set; } = int.MaxValue;
        public int PathHeuristic { get; set; } = int.MaxValue;
        public PathQueueNode Predecessor { get; set; } = null;

        public PathQueueNode(int x, int y) {
            this.X = x;
            this.Y = y;
            Distance = Math.Abs(x) + Math.Abs(y);
        }
    }
}
