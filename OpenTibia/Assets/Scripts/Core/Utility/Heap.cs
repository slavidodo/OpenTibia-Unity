using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utility
{
    public class Heap
    {
        // TODO:
        // Technically, we are forcing a usage of <int> in a lis
        // while we could use a workarround to use SortedDictionary instead
        public List<HeapItem> m_Heap = null;

        public int Length { get; private set; } = 0;

        public Heap(int capacity = 0) {
            m_Heap = new List<HeapItem>(capacity);
        }

        public HeapItem AddItem(HeapItem item, int key) {
            if (item != null) {
                int len = Length;
                item.HeapKey = key;
                item.HeapParent = this;
                item.HeapPosition = len;
                m_Heap.Add(item);
                Length++;
                MinHeapify(len);
            }
            return item;
        }

        public HeapItem UpdateKey(HeapItem heapItem, int key) {
            if (heapItem != null && heapItem.HeapParent == this && heapItem.HeapPosition < Length && heapItem.HeapKey != key) {
                heapItem.HeapKey = key;
                MinHeapify(heapItem.HeapPosition);
            }
            return heapItem;
        }

        public void MinHeapify(int len, bool param2 = true) {
            int index = len;
            while (param2 && index > 0 && index < m_Heap.Count - 2) {
                int nextIndex = index + 1;
                if (m_Heap[index].HeapKey < m_Heap[nextIndex].HeapKey) {
                    var nextHeap = m_Heap[nextIndex];
                    m_Heap[nextIndex] = m_Heap[index];
                    m_Heap[nextIndex].HeapPosition = nextIndex;
                    m_Heap[index] = nextHeap;
                    m_Heap[index].HeapPosition = index;
                    index = nextIndex;
                } else {
                    index = 0;
                }
            }

            while (true) {
                int loc5 = index + 1;
                int loc6 = loc5 + 1;
                int loc7 = index;
                if (loc5 < Length && m_Heap[loc5].HeapKey < m_Heap[loc7].HeapKey)
                    loc7 = loc5;

                if (loc6 < Length && m_Heap[loc6].HeapKey < m_Heap[loc7].HeapKey)
                    loc7 = loc6;

                if (loc7 > index) {
                    var otherHeap = m_Heap[index];
                    m_Heap[index] = m_Heap[loc7];
                    m_Heap[index].HeapPosition = index;
                    m_Heap[loc7] = otherHeap;
                    m_Heap[loc7].HeapPosition = loc7;
                    index = loc7;
                    continue;
                }
                break;
            }
        }

        public HeapItem PeekMinItem() {
            return Length > 0 ? m_Heap[0] : null;
        }

        public HeapItem ExtractMinItem() {
            if (Length <= 0)
                return null;

            var minHeap = m_Heap[0];
            minHeap.HeapParent = null;
            minHeap.HeapPosition = -1;
            Length--;
            m_Heap[0] = m_Heap[Length];
            m_Heap.RemoveAt(Length);
            MinHeapify(0);
            return minHeap;
        }

        public void Clear(bool full = true) {
            HeapItem heapItem = null;
            var index = 0;
            for (int i = 0; i < Length; i++) {
                if (heapItem != null) {
                    heapItem.HeapPosition = -1;
                    heapItem.HeapParent = null;
                }
                index++;
            }

            Length = 0;
            if (full)
                m_Heap.Clear();
        }
    }
}
