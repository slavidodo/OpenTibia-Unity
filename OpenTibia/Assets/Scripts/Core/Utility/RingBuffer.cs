using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utility
{
    public class RingBuffer<T> : IEnumerable<T>
    {
        private T[] m_Data;
        private int m_Offset;
        private int m_Length;
        private int m_Size;

        public int Size {
            get { return m_Size; }
        }

        public int Length {
            get { return m_Length; }
        }

        public RingBuffer(int capacity = 1) {
            if (capacity < 1)
                throw new ArgumentException("RingBuffer.RingBuffer: Size has to be >= 1.");

            m_Size = capacity;
            m_Data = new T[m_Size];
            m_Offset = 0;
            m_Length = 0;
        }

        public void RemoveAll() {
            m_Data = new T[m_Size];
            m_Offset = 0;
            m_Length = 0;
        }

        public int GetItemIndex(T item) {
            for (int i = 0; i < m_Length; i++) {
                if (m_Data[(m_Offset + i) % m_Size].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        public T RemoveItemAt(int index) {
            if (index < 0 || index >= m_Length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            T item = m_Data[(m_Offset + index) % m_Size];
            int localIndex = index;
            while (localIndex < m_Length - 1) {
                m_Data[(m_Offset + localIndex) % m_Size] = m_Data[(m_Offset + localIndex + 1) % m_Size];
                localIndex++;
            }
            m_Length--;
            return item;
        }

        public T AddItem(T item) {
            return AddItemInternal(item);
        }

        public T GetItemAt(int index) {
            if (index < 0 || index >= m_Length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            return m_Data[(m_Offset + index) % m_Size];
        }

        public T[] ToArray() {
            var array = new T[m_Length];
            int index = 0;
            while (index < m_Length) {
                array[index] = m_Data[(m_Offset + index) %m_Size];
                index++;
            }

            return array;
        }

        public void AddItemAt(T item, int index) {
            if (index < 0 || index >= m_Length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            if (m_Length < m_Size) {
                var localIndex = m_Length;
                while (localIndex > index) {
                    m_Data[localIndex] = m_Data[localIndex - 1];
                    localIndex--;
                }

                m_Data[index] = item;
                m_Length++;
            } else {
                var otherItem = m_Data[m_Offset];
                index = Math.Min(index, m_Size - 1);
                int i = 0;
                while (i < index) {
                    m_Data[(m_Offset + i) % m_Size] = m_Data[(m_Offset + i + 1) % m_Size];
                    i++;
                }

                m_Data[(m_Offset + index) % m_Size] = item;
            }
        }

        public T AddItemInternal(T item) {
            T removedItem = default;
            if (m_Length < m_Size) {
                m_Data[(m_Offset + m_Length) % m_Size] = item;
                m_Length++;
            } else {
                removedItem = m_Data[m_Offset];
                m_Data[m_Offset] = item;
                m_Offset = (m_Offset + 1) % m_Size;
            }

            return removedItem;
        }

        public T SetitemAt(T item, int index) {
            if (index < 0 || index >= m_Length)
                throw new IndexOutOfRangeException("RingBuffer.removeItemAt: Index " + index + " is out of range.");

            int localIndex = (m_Offset + index) % m_Size;
            T other = m_Data[localIndex];
            m_Data[localIndex] = other;
            return other;
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < m_Length; i++)
                yield return GetItemAt(i);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
