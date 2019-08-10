using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Container
{
    internal class ContainerView {
        internal class ContainerViewObjectEvent : UnityEvent<ContainerView, int, ObjectInstance> {}
        
        private int m_ID = 0;
        private int m_NumberOfSlotsPerPage = 0;
        private int m_NumberOfTotalObjects = 0;
        private int m_IndexOfFirstObject = 0;
        private bool m_IsSubContainer = false;
        private bool m_IsDragAndDropEnabled = false;
        private bool m_IsPaginationEnabled = false;
        
        private string m_Name;
        private ObjectInstance m_Icon;
        private List<ObjectInstance> m_Objects;

        internal ContainerViewEvent onRemoveAll = new ContainerViewEvent();

        internal ContainerViewObjectEvent onObjectAdded = new ContainerViewObjectEvent();
        internal ContainerViewObjectEvent onObjectChanged = new ContainerViewObjectEvent();
        internal ContainerViewObjectEvent onObjectRemoved = new ContainerViewObjectEvent();

        internal int ID { get => m_ID; }
        internal int NumberOfSlotsPerPage { get => m_NumberOfSlotsPerPage; }
        internal int NumberOfTotalObjects { get => m_NumberOfTotalObjects; }
        internal int IndexOfFirstObject { get => m_IndexOfFirstObject; }

        internal string Name { get => m_Name; }

        internal bool IsSubContainer { get => m_IsSubContainer; }
        internal bool IsDragAndDropEnabled { get => m_IsDragAndDropEnabled; }
        internal bool IsPaginationEnabled { get => m_IsPaginationEnabled; }

        internal ContainerView(int id, ObjectInstance icon, string name, bool subContainer, bool isDragAndDropEnabled, bool isPaginatopnEnabled, int nOfSlots, int nOfObjects, int indexOfFirstObject) {
            if (id < 0 || id >= Constants.MaxContainerViews)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid ID: " + id);

            if (name == null || name.Length == 0 || name.Length >= Constants.MaxContainerNameLength)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid name: " + name);

            if (nOfSlots <= 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid number of slots per page: " + nOfSlots);

            if (indexOfFirstObject < 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid index of first object(1): " + indexOfFirstObject);

            if (isPaginatopnEnabled && indexOfFirstObject % nOfSlots != 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid index of first object(2): " + indexOfFirstObject);

            m_ID = id;
            m_Icon = icon ?? throw new System.ArgumentException("ContainerView.ContainerView: Invalid icon for a container.");
            m_Name = name;
            m_IsSubContainer = subContainer;
            m_IsDragAndDropEnabled = isDragAndDropEnabled;
            m_IsPaginationEnabled = isPaginatopnEnabled;
            m_NumberOfSlotsPerPage = nOfSlots;
            m_NumberOfTotalObjects = nOfObjects;
            m_IndexOfFirstObject = indexOfFirstObject;

            m_Objects = new List<ObjectInstance>();
        }

        internal ObjectInstance GetObject(int index) {
            if (index < m_IndexOfFirstObject || index >= m_IndexOfFirstObject + m_Objects.Count)
                throw new System.IndexOutOfRangeException("ContainerView.changeObject: Index out of range: " + index);

            return m_Objects[index - m_IndexOfFirstObject];
        }

        internal void AddObject(int index, ObjectInstance @object) {
            if (@object != null) {
                if (index < m_IndexOfFirstObject || index > m_IndexOfFirstObject + m_Objects.Count)
                    throw new System.IndexOutOfRangeException("ContainerView.AddObject: Index out of range: " + index);

                if (m_NumberOfSlotsPerPage <= m_Objects.Count)
                    m_Objects.RemoveAt(0);

                m_Objects.Insert(index - m_IndexOfFirstObject, @object);
            }

            m_NumberOfTotalObjects++;

            onObjectAdded.Invoke(this, index, @object);
        }

        internal void ChangeObject(int index, ObjectInstance @object) {
            if (index < m_IndexOfFirstObject || index >= m_IndexOfFirstObject + m_Objects.Count)
                throw new System.IndexOutOfRangeException("ContainerView.changeObject: Index out of range: " + index);

            m_Objects[index - m_IndexOfFirstObject] = @object ?? throw new System.ArgumentNullException("ContainerView.changeObject: Invalid object: " + @object);

            onObjectChanged.Invoke(this, index, @object);
        }
        
        internal void RemoveObject(int index, ObjectInstance appendObject) {
            if (index < 0 || index >= m_NumberOfTotalObjects)
                throw new System.IndexOutOfRangeException("ContainerView.removeObject: Index out of range: " + index);

            if (index >= m_IndexOfFirstObject && index < m_IndexOfFirstObject + m_Objects.Count)
                m_Objects.RemoveAt(index - m_IndexOfFirstObject);

            if (appendObject != null)
                m_Objects.Add(appendObject);

            m_NumberOfTotalObjects--;

            onObjectRemoved.Invoke(this, index, appendObject);
        }

        internal void RemoveAll() {
            m_NumberOfTotalObjects -= m_Objects.Count;
            m_Objects.Clear();

            onRemoveAll.Invoke(this);
        }

        public static bool operator !(ContainerView instance) {
            return instance == null;
        }

        public static bool operator true(ContainerView instance) {
            return !!instance;
        }

        public static bool operator false(ContainerView instance) {
            return !instance;
        }
    }
}
