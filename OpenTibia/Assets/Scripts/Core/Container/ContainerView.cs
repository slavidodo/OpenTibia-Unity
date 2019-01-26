using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Container
{
    public class ContainerView
    {

        private int m_ID = 0;
        private int m_NumberOfTotalObjects = 0;
        private int m_NumberOfSlotsPerPage = 0;
        private int m_IndexOfFirstObject = 0;
        private bool m_IsDragAndDropEnabled = false;
        private bool m_IsPaginationEnabled = false;
        private bool m_IsSubContainer = false;

        private ObjectInstance m_Icon = null;

        private string m_Name = null;
        private List<ObjectInstance> m_Objects = new List<ObjectInstance>();


        public ContainerView(int id, ObjectInstance icon, string name, bool subContainer, bool dragAndDrop, bool pagination, int nOfSlots, int nOfObjects, int indexOfFirstObject) {
            if (id < 0 || id >= Constants.MaxContainerViews)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid ID: " + id);

            if (name == null || name.Length == 0 || name.Length >= Constants.MaxContainerNameLength)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid name: " + name);

            if (nOfSlots <= 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid number of slots per page: " + nOfSlots);

            if (indexOfFirstObject < 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid index of first object(1): " + indexOfFirstObject);

            if (pagination && indexOfFirstObject % nOfSlots != 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid index of first object(2): " + indexOfFirstObject);

            m_ID = id;
            m_Icon = icon ?? throw new System.ArgumentException("ContainerView.ContainerView: Invalid icon: " + icon);
            m_Name = name;
            m_IsSubContainer = subContainer;
            m_IsDragAndDropEnabled = dragAndDrop;
            m_IsPaginationEnabled = pagination;
            m_NumberOfSlotsPerPage = nOfSlots;
            m_NumberOfTotalObjects = nOfObjects;
            m_IndexOfFirstObject = indexOfFirstObject;
        }

        public void AddObject(int index, ObjectInstance obj) {
            if (obj != null) {
                if (index < m_IndexOfFirstObject || index > m_IndexOfFirstObject + m_Objects.Count) {
                    throw new System.IndexOutOfRangeException("ContainerView.addObject: Index out of range: " + index);
                }

                if (m_NumberOfSlotsPerPage <= m_Objects.Count)
                    m_Objects.RemoveAt(0);

                m_Objects.Insert(index - m_IndexOfFirstObject, obj);
            }

            m_NumberOfTotalObjects++;
            // TODO: dispatch event
        }

        public void ChangeObject(int index, ObjectInstance obj) {
            if (index < m_IndexOfFirstObject || index >= m_IndexOfFirstObject + m_Objects.Count)
                throw new System.IndexOutOfRangeException("ContainerView.changeObject: Index out of range: " + index);

            m_Objects[index - m_IndexOfFirstObject] = obj ?? throw new System.ArgumentNullException("ContainerView.changeObject: Invalid object: " + obj);

            // TODO: dispatch event
        }

        public ObjectInstance GetObject(int index) {
            if (index < m_IndexOfFirstObject || index >= m_IndexOfFirstObject + m_Objects.Count)
                throw new System.IndexOutOfRangeException("ContainerView.changeObject: Index out of range: " + index);

            return m_Objects[index - m_IndexOfFirstObject];
        }

        public void RemoveObject(int index, ObjectInstance obj) {
            // TODO
            if (index < 0 || index >= m_NumberOfTotalObjects)
                throw new System.IndexOutOfRangeException("ContainerView.removeObject: Index out of range: " + index);

            if (index >= m_IndexOfFirstObject && index < m_IndexOfFirstObject + m_Objects.Count)
                m_Objects.RemoveAt(index - m_IndexOfFirstObject);

            if (obj != null)
                m_Objects.Add(obj);

            m_NumberOfTotalObjects--;
            // TODO: dispatch event
        }

        public void RemoveAll() {
            m_NumberOfTotalObjects -= m_Objects.Count;
            m_Objects.Clear();
            // TODO: dispatch event
        }
    }
}
