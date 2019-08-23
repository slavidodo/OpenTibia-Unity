using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utils
{
    public class RingBuffer<T> : IEnumerable<T>
    {
        private T[] _data;
        private int _offset;
        private int _length;
        private int _size;

        public int Size {
            get { return _size; }
        }

        public int Length {
            get { return _length; }
        }

        public RingBuffer(int capacity = 1) {
            if (capacity < 1)
                throw new ArgumentException("RingBuffer.RingBuffer: Size has to be >= 1.");

            _size = capacity;
            _data = new T[_size];
            _offset = 0;
            _length = 0;
        }

        public void RemoveAll() {
            _data = new T[_size];
            _offset = 0;
            _length = 0;
        }

        public int GetItemIndex(T item) {
            for (int i = 0; i < _length; i++) {
                if (_data[(_offset + i) % _size].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        public T RemoveItemAt(int index) {
            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            T item = _data[(_offset + index) % _size];
            int localIndex = index;
            while (localIndex < _length - 1) {
                _data[(_offset + localIndex) % _size] = _data[(_offset + localIndex + 1) % _size];
                localIndex++;
            }
            _length--;
            return item;
        }

        public T AddItem(T item) {
            return AddItempublic(item);
        }

        public T GetItemAt(int index) {
            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            return _data[(_offset + index) % _size];
        }

        public T[] ToArray() {
            var array = new T[_length];
            int index = 0;
            while (index < _length) {
                array[index] = _data[(_offset + index) %_size];
                index++;
            }

            return array;
        }

        public void AddItemAt(T item, int index) {
            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            if (_length < _size) {
                var localIndex = _length;
                while (localIndex > index) {
                    _data[localIndex] = _data[localIndex - 1];
                    localIndex--;
                }

                _data[index] = item;
                _length++;
            } else {
                var otherItem = _data[_offset];
                index = Math.Min(index, _size - 1);
                int i = 0;
                while (i < index) {
                    _data[(_offset + i) % _size] = _data[(_offset + i + 1) % _size];
                    i++;
                }

                _data[(_offset + index) % _size] = item;
            }
        }

        public T AddItempublic(T item) {
            T removedItem = default;
            if (_length < _size) {
                _data[(_offset + _length) % _size] = item;
                _length++;
            } else {
                removedItem = _data[_offset];
                _data[_offset] = item;
                _offset = (_offset + 1) % _size;
            }

            return removedItem;
        }

        public T SetitemAt(T item, int index) {
            if (index < 0 || index >= _length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            int localIndex = (_offset + index) % _size;
            T other = _data[localIndex];
            _data[localIndex] = other;
            return other;
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < _length; i++)
                yield return GetItemAt(i);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
