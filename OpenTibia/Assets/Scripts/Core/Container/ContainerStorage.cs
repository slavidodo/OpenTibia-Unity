using OpenTibiaUnity.Core.Appearances;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Container
{
    public class ContainerStorage
    {
        private long m_PlayerMoney = 0;
        private Utility.Delay m_MultiuseDelay = new Utility.Delay(0, 0);
        private List<InventoryTypeInfo> m_PlayerInventory = new List<InventoryTypeInfo>();
        private List<InventoryTypeInfo> m_PlayerGoods = new List<InventoryTypeInfo>();
        private List<ContainerView> m_ContainerViews = new List<ContainerView>(Constants.MaxContainerViews);

        public BodyContainerView BodyContainerView { get; set; }

        public ContainerStorage() {
            BodyContainerView = new BodyContainerView();
        }

        public void Reset() {

        }

        public int GetAvailableInventory(uint id, int data) {
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

        public int GetFreeContainerViewID() {
            int index = 0;
            while (index < Constants.MaxContainerViews) {
                if (m_ContainerViews[index] == null)
                    return index;
                index++;
            }
            return Constants.MaxContainerViews - index;
        }

        public void SetPlayerMoney(long playerMoney) {
            if (m_PlayerMoney != playerMoney) {
                m_PlayerMoney = playerMoney;
                // TODO dispatch event
            }
        }

        public void CreateContainerView(int id, ObjectInstance icon, string name, bool subContainer, bool dragAndDrop, bool pagination, int nOfSlots, int nOfObjects, int indexOfFirstObject) {
            var containerView = new ContainerView(id, icon, name, subContainer, dragAndDrop, pagination, nOfSlots, nOfObjects, indexOfFirstObject);

            m_ContainerViews[id] = containerView;
        }
    }
}
