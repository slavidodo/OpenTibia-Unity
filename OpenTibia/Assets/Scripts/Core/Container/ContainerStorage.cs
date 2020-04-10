using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Container
{
    public class ContainerViewEvent : UnityEvent<ContainerView> { }
    public class ContainerViewAddEvent : UnityEvent<ContainerView, int> { }
    public class PlayerMoneyEvent : UnityEvent<long, long> { }
    public class PlayerInventoryEvent : UnityEvent<List<InventoryTypeInfo>> { }

    public class ContainerStorage
    {
        private Utils.Delay _multiuseDelay = new Utils.Delay(0, 0);
        private ContainerView[] _bontainerViews = new ContainerView[Constants.MaxContainerViews];

        public ContainerViewAddEvent onContainerAdded = new ContainerViewAddEvent();
        public ContainerViewEvent onContainerClosed = new ContainerViewEvent();

        public PlayerMoneyEvent onPlayerMoneyChange = new PlayerMoneyEvent();
        public PlayerInventoryEvent onPlayerInventoryChange = new PlayerInventoryEvent();
        public PlayerInventoryEvent onPlayerGoodsChange = new PlayerInventoryEvent();

        private long _playerMoney = 0;
        public long PlayerMoney {
            get { return _playerMoney; }
            set { var oldMoney = _playerMoney; _playerMoney = value; onPlayerMoneyChange.Invoke(oldMoney, _playerMoney); }
        }

        private List<InventoryTypeInfo> _playerInventory = new List<InventoryTypeInfo>();
        public List<InventoryTypeInfo> PlayerInventory {
            get { return _playerInventory; }
            set {
                if (value == null)
                    throw new System.ArgumentNullException("ContainerStorage.SetPlayerGoods: Invalid goods.");

                if (_playerInventory != value) {
                    _playerInventory = value;
                    onPlayerInventoryChange.Invoke(_playerInventory);
                }
            }
        }

        private List<InventoryTypeInfo> _playerGoods = new List<InventoryTypeInfo>();
        public List<InventoryTypeInfo> PlayerGoods {
            get { return _playerGoods; }
            set {
                if (value == null)
                    throw new System.ArgumentNullException("ContainerStorage.SetPlayerGoods: Invalid goods.");

                if (_playerGoods != value) {
                    _playerGoods = value;
                    onPlayerGoodsChange.Invoke(_playerGoods);
                }
            }
        }

        public BodyContainerView BodyContainerView { get; set; }

        public ContainerStorage() {
            BodyContainerView = new BodyContainerView();
        }

        public void Reset() {

        }

        public int GetAvailableInventory(uint id, int data) {
            int l = 0, r = _playerInventory.Count - 1;
            while (l <= r) {
                int i = l + r >> 1;
                var typeInfo = _playerInventory[i];
                int direction = typeInfo.Compare((int)id, data);
                if (direction < 0)
                    l = i + 1;
                else if (direction > 0)
                    r = i - 1;
                else
                    return typeInfo.Count;
            }

            return 0;
        }

        public int GetFreeContainerViewId() {
            for (int i = 0; i < Constants.MaxContainerViews; i++) {
                if (_bontainerViews[i] == null)
                    return i;
            }

            return 0;
        }

        public ContainerView GetContainerView(int containerId) {
            if (containerId < 0 || containerId >= Constants.MaxContainerViews)
                throw new System.ArgumentOutOfRangeException("ContainerStorage.GetContainerView: Invalid container number: " + containerId);

            return _bontainerViews[containerId];
        }

        public ContainerView CreateContainerView(int containerId, ObjectInstance objectIcon, string name, bool isSubContainer, bool isDragAndDropEnabled,
                                                 bool isPaginationEnabled, int nOfSlotsPerPage, int nOfTotalObjects, int indexOfFirstObject,
                                                 int expectedNOfObjects) {
            var containerView = new ContainerView(containerId, objectIcon, name, isSubContainer, isDragAndDropEnabled, isPaginationEnabled, nOfSlotsPerPage, nOfTotalObjects, indexOfFirstObject);

            _bontainerViews[containerId] = containerView;
            onContainerAdded.Invoke(containerView, expectedNOfObjects);

            return containerView;
        }

        public void CloseContainerView(int containerId) {
            if (containerId < 0 || containerId >= Constants.MaxContainerViews)
                throw new System.ArgumentOutOfRangeException("ContainerStorage.GetContainerView: Invalid container number: " + containerId);
            
            var containerView = _bontainerViews[containerId];
            _bontainerViews[containerId] = null;

            if (!!containerView)
                onContainerClosed.Invoke(containerView);
        }
    }
}
