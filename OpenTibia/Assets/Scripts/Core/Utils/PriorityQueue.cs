using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utils
{
    public class PriorityQueue<T>
    {
        private IComparer<T> _comparer;
        private List<T> _internalHeap = new List<T>();

        public int Count { get => _internalHeap.Count; }

        public PriorityQueue() {
            _comparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer) {
            _comparer = comparer;
        }

        public PriorityQueue(IComparer<T> comparer, int capacity) {
            _comparer = comparer;
            _internalHeap.Capacity = capacity;
        }

        public void Push(T item) {
            int index = _internalHeap.Count;
            _internalHeap.Add(item);

            int parent;
            while (index > 0) {
                parent = (index - 1) / 2;
                if (Compare(index, parent) < 0) {
                    Swap(index, parent);
                    index = parent;
                } else {
                    break;
                }
            }
        }

        // extract min
        public T Pop() {
            // avoid undefined behaviour
            if (Count == 0)
                return default;

            T root = _internalHeap[0];
            _internalHeap[0] = _internalHeap[_internalHeap.Count - 1];
            _internalHeap.RemoveAt(_internalHeap.Count - 1);

            if (_internalHeap.Count > 0)
                MinHeapify(0);
            return root;
        }

        // notify the pq that the object at index has changed
        // just minheapify
        public void Update(int i) {
            int index = i, parent;
            while (index > 0) {
                parent = (index - 1) / 2;
                if (Compare(index, parent) < 0) {
                    Swap(index, parent);
                    index = parent;
                } else {
                    break;
                }
            }

            if (index < i)
                return;

            MinHeapify(i);
        }

        private int Compare(int i, int j) {
            return _comparer.Compare(_internalHeap[i], _internalHeap[j]);
        }

        private void Swap(int i, int j) {
            T temp = _internalHeap[i];
            _internalHeap[i] = _internalHeap[j];
            _internalHeap[j] = temp;
        }

        private void MinHeapify(int index = 0) {
            int left, right, smallest;
            while (true) {
                smallest = index;
                left = 2 * index + 1;
                right = 2 * index + 2;
                if (_internalHeap.Count > left && Compare(index, left) > 0)
                    index = left;
                if (_internalHeap.Count > right && Compare(index, right) > 0)
                    index = right;
                if (index == smallest)
                    break;
                Swap(index, smallest);
            }
        }
    }
}
