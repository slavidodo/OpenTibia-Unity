using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utils
{
    public class Heap
    {
        // TODO:
        // Technically, we are forcing a usage of <int> in a lis
        // while we could use a workarround to use SortedDictionary instead
        public List<HeapItem> _heap = null;

        public int Length { get; private set; } = 0;

        public Heap(int capacity = 0) {
            _heap = new List<HeapItem>(capacity);
        }

        public HeapItem AddItem(HeapItem item, int key) {
            if (item != null) {
                int len = Length;
                item.HeapKey = key;
                item.HeapParent = this;
                item.HeapPosition = len;
                _heap.Add(item);
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
            while (param2 && index > 0 && index < _heap.Count - 2) {
                int nextIndex = index + 1;
                if (_heap[index].HeapKey < _heap[nextIndex].HeapKey) {
                    var nextHeap = _heap[nextIndex];
                    _heap[nextIndex] = _heap[index];
                    _heap[nextIndex].HeapPosition = nextIndex;
                    _heap[index] = nextHeap;
                    _heap[index].HeapPosition = index;
                    index = nextIndex;
                } else {
                    index = 0;
                }
            }

            while (true) {
                int loc5 = index + 1;
                int loc6 = loc5 + 1;
                int loc7 = index;
                if (loc5 < Length && _heap[loc5].HeapKey < _heap[loc7].HeapKey)
                    loc7 = loc5;

                if (loc6 < Length && _heap[loc6].HeapKey < _heap[loc7].HeapKey)
                    loc7 = loc6;

                if (loc7 > index) {
                    var otherHeap = _heap[index];
                    _heap[index] = _heap[loc7];
                    _heap[index].HeapPosition = index;
                    _heap[loc7] = otherHeap;
                    _heap[loc7].HeapPosition = loc7;
                    index = loc7;
                    continue;
                }
                break;
            }
        }

        public HeapItem PeekMinItem() {
            return Length > 0 ? _heap[0] : null;
        }

        public HeapItem ExtractMinItem() {
            if (Length <= 0)
                return null;

            var minHeap = _heap[0];
            minHeap.HeapParent = null;
            minHeap.HeapPosition = -1;
            Length--;
            _heap[0] = _heap[Length];
            _heap.RemoveAt(Length);
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
                _heap.Clear();
        }
    }
}
