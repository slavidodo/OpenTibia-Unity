using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseOpenContainer(Internal.CommunicationStream message) {
            byte containerId = message.ReadUnsignedByte();
            var objectIcon = ProtocolGameExtentions.ReadObjectInstance(message);
            string name = message.ReadString();
            byte nOfSlotsPerPage = message.ReadUnsignedByte(); // capacity of shown view
            bool isSubContainer = message.ReadBoolean();

            bool canUseDepotSearch = false;
            bool isDragAndDropEnabled = true;
            bool isPaginationEnabled = false;
            int nOfTotalObjects;
            int indexOfFirstObject = 0;
            int nOfContentObjects; // objects in the current shown view //
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameContainerPagination)) {
                if (OpenTibiaUnity.GameManager.ClientVersion >= 1220)
                    canUseDepotSearch = message.ReadBoolean();

                isDragAndDropEnabled = message.ReadBoolean();
                isPaginationEnabled = message.ReadBoolean();
                nOfTotalObjects = message.ReadUnsignedShort();
                indexOfFirstObject = message.ReadUnsignedShort();
                nOfContentObjects = message.ReadUnsignedByte();

                if (nOfContentObjects > nOfSlotsPerPage)
                    throw new System.Exception("ProtocolGame.ParseOpenContainer: Number of content objects " + nOfContentObjects + " exceeds number of slots per page " + nOfSlotsPerPage);

                if (nOfContentObjects > nOfTotalObjects)
                    throw new System.Exception("ProtocolGame.ParseOpenContainer: Number of content objects " + nOfContentObjects + " exceeds number of total objects " + nOfTotalObjects);
            } else {
                nOfContentObjects = message.ReadUnsignedByte();
                nOfTotalObjects = nOfContentObjects;

                if (nOfContentObjects > nOfSlotsPerPage)
                    throw new System.Exception("ProtocolGame.ParseOpenContainer: Number of content objects " + nOfContentObjects + " exceeds the capaciy " + nOfSlotsPerPage);
            }

            var containerView = ContainerStorage.CreateContainerView(containerId, objectIcon, name, isSubContainer,
                                    isDragAndDropEnabled, isPaginationEnabled, nOfSlotsPerPage,
                                    nOfTotalObjects - nOfContentObjects, indexOfFirstObject, nOfContentObjects);

            for (int i = 0; i < nOfContentObjects; i++)
                containerView.AddObject(indexOfFirstObject + i, ProtocolGameExtentions.ReadObjectInstance(message));
        }

        private void ParseCloseContainer(Internal.CommunicationStream message) {
            byte containerId = message.ReadUnsignedByte();
            ContainerStorage.CloseContainerView(containerId);
        }

        private void ParseCreateInContainer(Internal.CommunicationStream message) {
            byte containerId = message.ReadUnsignedByte();
            ushort slot = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameContainerPagination))
                slot = message.ReadUnsignedShort();
            var @object = ProtocolGameExtentions.ReadObjectInstance(message);

            var containerView = ContainerStorage.GetContainerView(containerId);
            if (!!containerView)
                containerView.AddObject(slot, @object);
        }

        private void ParseChangeInContainer(Internal.CommunicationStream message) {
            byte containerId = message.ReadUnsignedByte();
            ushort slot = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameContainerPagination))
                slot = message.ReadUnsignedShort();
            else
                slot = message.ReadUnsignedByte();
            var @object = ProtocolGameExtentions.ReadObjectInstance(message);

            var containerView = ContainerStorage.GetContainerView(containerId);
            if (!!containerView)
                containerView.ChangeObject(slot, @object);
        }

        private void ParseDeleteInContainer(Internal.CommunicationStream message) {
            byte containerId = message.ReadUnsignedByte();
            ushort slot;
            Appearances.ObjectInstance appendObject = null;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameContainerPagination)) {
                slot = message.ReadUnsignedShort();
                ushort itemId = message.ReadUnsignedShort();
                
                if (itemId != 0)
                    appendObject = ProtocolGameExtentions.ReadObjectInstance(message, itemId);
            } else {
                slot = message.ReadUnsignedByte();
            }

            var containerView = ContainerStorage.GetContainerView(containerId);
            if (!!containerView)
                containerView.RemoveObject(slot, appendObject);
        }
    }
}
