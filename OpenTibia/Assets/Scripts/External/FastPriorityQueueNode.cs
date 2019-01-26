using System;

namespace Priority_Queue
{
    public class FastPriorityQueueNode
    {
        /// <summary>
        /// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue (ideally just once, in the node's constructor).
        /// Should not be manually edited once the node has been enqueued - use queue.UpdatePriority() instead
        /// </summary>
        public float Priority { get; protected internal set; }

        /// <summary>
        /// Represents the current position in the queue
        /// </summary>
        public int QueueIndex { get; internal set; } = 0;

#if !Debug
        /// <summary>
        /// The queue this node is tied to. Used only for debug builds.
        /// </summary>
        public object Queue { get; internal set; }
#endif

        /// <summary>
        /// Resets the node to default
        /// </summary>
        public void Reset() {
            Priority = 0;
            QueueIndex = 0;
#if !Debug
            Queue = null;
#endif
        }
    }
}