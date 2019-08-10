using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Container
{
    internal class ContainerViewEvent : UnityEvent<ContainerView> { }
    internal class PlayerMoneyEvent : UnityEvent<long, long> { }
    internal class PlayerInventoryEvent : UnityEvent<List<InventoryTypeInfo>> { }

    internal class ContainerStorage
    {
        private Utility.Delay m_MultiuseDelay = new Utility.Delay(0, 0);
        private ContainerView[] m_ContainerViews = new ContainerView[Constants.MaxContainerViews];

        internal ContainerViewEvent onContainerAdded = new ContainerViewEvent();
        internal ContainerViewEvent onContainerClosed = new ContainerViewEvent();

        internal PlayerMoneyEvent onPlayerMoneyChange = new PlayerMoneyEvent();
        internal PlayerInventoryEvent onPlayerInventoryChange = new PlayerInventoryEvent();
        internal PlayerInventoryEvent onPlayerGoodsChange = new PlayerInventoryEvent();

        private long m_PlayerMoney = 0;
        internal long PlayerMoney {
            get { return m_PlayerMoney; }
            set { var oldMoney = m_PlayerMoney; m_PlayerMoney = value; onPlayerMoneyChange.Invoke(oldMoney, m_PlayerMoney); }
        }

        private List<InventoryTypeInfo> m_PlayerInventory = new List<InventoryTypeInfo>();
        internal List<InventoryTypeInfo> PlayerInventory {
            get { return m_PlayerInventory; }
            set {
                if (value == null)
                    throw new System.ArgumentNullException("ContainerStorage.SetPlayerGoods: Invalid goods.");

                if (m_PlayerInventory != value) {
                    m_PlayerInventory = value;
                    onPlayerInventoryChange.Invoke(m_PlayerInventory);
                }
            }
        }

        private List<InventoryTypeInfo> m_PlayerGoods = new List<InventoryTypeInfo>();
        internal List<InventoryTypeInfo> PlayerGoods {
            get { return m_PlayerGoods; }
            set {
                if (value == null)
                    throw new System.ArgumentNullException("ContainerStorage.SetPlayerGoods: Invalid goods.");

                if (m_PlayerGoods != value) {
                    m_PlayerGoods = value;
                    onPlayerGoodsChange.Invoke(m_PlayerGoods);
                }
            }
        }

        internal BodyContainerView BodyContainerView { get; set; }

        internal ContainerStorage() {
            BodyContainerView = new BodyContainerView();
        }

        internal void Reset() {

        }

        internal int GetAvailableInventory(uint id, int data) {
            int lastIndex = m_PlayerInventory.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var typeInfo = m_PlayerInventory[tmpIndex];
                int direction = typeInfo.Compare((int)id, data);
                if (direction < 0)
                    index = tmpIndex + 1;
                else if (direction > 0)
                    lastIndex = tmpIndex - 1;
                else
                    return typeInfo.Count;
            }

            return 0;
        }

        internal int GetFreeContainerViewID() {
            for (int i = 0; i < Constants.MaxContainerViews; i++) {
                if (m_ContainerViews[i] == null)
                    return i;
            }

            return 0;
        }

        internal ContainerView GetContainerView(int containerId) {
            if (containerId < 0 || containerId >= Constants.MaxContainerViews)
                throw new System.ArgumentOutOfRangeException("ContainerStorage.GetContainerView: Invalid container number: " + containerId);

            return m_ContainerViews[containerId];
        }

        internal ContainerView CreateContainerView(int containerId, ObjectInstance objectIcon, string name, bool isSubContainer, bool isDragAndDropEnabled, bool isPaginationEnabled, int nOfSlotsPerPage, int nOfTotalObjects, int indexOfFirstObject) {
            var containerView = new ContainerView(containerId, objectIcon, name, isSubContainer, isDragAndDropEnabled, isPaginationEnabled, nOfSlotsPerPage, nOfTotalObjects, indexOfFirstObject);
            
            m_ContainerViews[containerId] = containerView;
            onContainerAdded.Invoke(containerView);

            return containerView;
        }

        internal void CloseContainerView(int containerId) {
            if (containerId < 0 || containerId >= Constants.MaxContainerViews)
                throw new System.ArgumentOutOfRangeException("ContainerStorage.GetContainerView: Invalid container number: " + containerId);
            
            var containerView = m_ContainerViews[containerId];
            m_ContainerViews[containerId] = null;

            if (!!containerView)
                onContainerClosed.Invoke(containerView);
        }
    }
}
