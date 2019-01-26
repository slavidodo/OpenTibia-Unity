using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseOpenContainer(InputMessage message) {
            byte containerId = message.GetU8();
            Appearances.ObjectInstance item = ReadObjectInstance(message);
            string name = message.GetString();
            byte capacity = message.GetU8();
            bool hasParent = message.GetBool();
            bool unlocked = message.GetBool();
            bool hasPages = message.GetBool();
            ushort containerSize = message.GetU16();
            ushort firstIndex = message.GetU16();

            int itemCount = message.GetU8();

            List<Appearances.ObjectInstance> items = new List<Appearances.ObjectInstance>(itemCount);
            for (int i = 0; i < itemCount; i++) {
                items.Add(ReadObjectInstance(message));
            }
        }

        private void ParseCloseContainer(InputMessage message) {
            byte containerId = message.GetU8();
        }

        private void ParseContainerAddItem(InputMessage message) {
            byte containerId = message.GetU8();
            ushort slot = message.GetU16();
            Appearances.ObjectInstance item = ReadObjectInstance(message);
        }

        private void ParseContainerUpdateItem(InputMessage message) {
            byte containerId = message.GetU8();
            ushort slot = message.GetU16();
            Appearances.ObjectInstance item = ReadObjectInstance(message);
        }

        private void ParseContainerRemoveItem(InputMessage message) {
            byte containerId = message.GetU8();
            ushort slot = message.GetU16();

            ushort itemId = message.GetU16();
            Appearances.ObjectInstance item;
            if (itemId != 0)
                item = ReadObjectInstance(message);
            else
                item = null;
        }
    }
}
