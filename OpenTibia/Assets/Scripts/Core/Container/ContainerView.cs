using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Container
{
    public class ContainerView {
        public class ContainerViewObjectEvent : UnityEvent<ContainerView, int, ObjectInstance> {}
        
        private int _id = 0;
        private int _numberOfSlotsPerPage = 0;
        private int _numberOfTotalObjects = 0;
        private int _indexOfFirstObject = 0;
        private bool _isSubContainer = false;
        private bool _isDragAndDropEnabled = false;
        private bool _isPaginationEnabled = false;
        
        private string _name;
        private ObjectInstance _icon;
        private List<ObjectInstance> _objects;

        public ContainerViewEvent onRemoveAll = new ContainerViewEvent();

        public ContainerViewObjectEvent onObjectAdded = new ContainerViewObjectEvent();
        public ContainerViewObjectEvent onObjectChanged = new ContainerViewObjectEvent();
        public ContainerViewObjectEvent onObjectRemoved = new ContainerViewObjectEvent();

        public int Id { get => _id; }
        public int NumberOfSlotsPerPage { get => _numberOfSlotsPerPage; }
        public int NumberOfTotalObjects { get => _numberOfTotalObjects; }
        public int IndexOfFirstObject { get => _indexOfFirstObject; }

        public string Name { get => _name; }
        public ObjectInstance Icon { get => _icon; }

        public bool IsSubContainer { get => _isSubContainer; }
        public bool IsDragAndDropEnabled { get => _isDragAndDropEnabled; }
        public bool IsPaginationEnabled { get => _isPaginationEnabled; }

        public ContainerView(int id, ObjectInstance icon, string name, bool subContainer, bool isDragAndDropEnabled, bool isPaginatopnEnabled, int nOfSlots, int nOfObjects, int indexOfFirstObject) {
            if (id < 0 || id >= Constants.MaxContainerViews)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid _id: " + id);

            if (name == null || name.Length == 0 || name.Length >= Constants.MaxContainerNameLength)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid name: " + name);

            if (nOfSlots <= 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid number of slots per page: " + nOfSlots);

            if (indexOfFirstObject < 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid index of first object(1): " + indexOfFirstObject);

            if (isPaginatopnEnabled && indexOfFirstObject % nOfSlots != 0)
                throw new System.ArgumentException("ContainerView.ContainerView: Invalid index of first object(2): " + indexOfFirstObject);

            _id = id;
            _icon = icon ?? throw new System.ArgumentException("ContainerView.ContainerView: Invalid icon for a container.");
            _name = name;
            _isSubContainer = subContainer;
            _isDragAndDropEnabled = isDragAndDropEnabled;
            _isPaginationEnabled = isPaginatopnEnabled;
            _numberOfSlotsPerPage = nOfSlots;
            _numberOfTotalObjects = nOfObjects;
            _indexOfFirstObject = indexOfFirstObject;

            _objects = new List<ObjectInstance>();
        }

        public ObjectInstance GetObject(int index) {
            if (index < _indexOfFirstObject || index >= _indexOfFirstObject + _objects.Count)
                throw new System.IndexOutOfRangeException("ContainerView.changeObject: Index out of range: " + index);

            return _objects[index - _indexOfFirstObject];
        }

        public void AddObject(int index, ObjectInstance @object) {
            if (@object != null) {
                if (index < _indexOfFirstObject || index > _indexOfFirstObject + _objects.Count)
                    throw new System.IndexOutOfRangeException("ContainerView.AddObject: Index out of range: " + index);

                if (_numberOfSlotsPerPage <= _objects.Count)
                    _objects.RemoveAt(0);

                _objects.Insert(index - _indexOfFirstObject, @object);
            }

            _numberOfTotalObjects++;

            onObjectAdded.Invoke(this, index, @object);
        }

        public void ChangeObject(int index, ObjectInstance @object) {
            if (index < _indexOfFirstObject || index >= _indexOfFirstObject + _objects.Count)
                throw new System.IndexOutOfRangeException("ContainerView.changeObject: Index out of range: " + index);

            _objects[index - _indexOfFirstObject] = @object ?? throw new System.ArgumentNullException("ContainerView.changeObject: Invalid object: " + @object);

            onObjectChanged.Invoke(this, index, @object);
        }
        
        public void RemoveObject(int index, ObjectInstance appendObject) {
            if (index < 0 || index >= _numberOfTotalObjects)
                throw new System.IndexOutOfRangeException("ContainerView.removeObject: Index out of range: " + index);

            if (index >= _indexOfFirstObject && index < _indexOfFirstObject + _objects.Count)
                _objects.RemoveAt(index - _indexOfFirstObject);

            if (appendObject != null)
                _objects.Add(appendObject);

            _numberOfTotalObjects--;

            onObjectRemoved.Invoke(this, index, appendObject);
        }

        public void RemoveAll() {
            _numberOfTotalObjects -= _objects.Count;
            _objects.Clear();

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
