namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        private void ParseInspectionList(Internal.ByteArray message) {
            bool isPlayer = message.ReadBoolean();

            int size = message.ReadUnsignedByte();
            for (int i = 0; i < size; i++) {
                var @object = ReadObjectInstance(message);
                if (isPlayer) {
                    var slot = message.ReadEnum<ClothSlots>();
                }


                int imbuementSlots = message.ReadUnsignedByte();
                for (int j = 0; j < imbuementSlots; j++) {
                    int imbuementID = message.ReadUnsignedByte();
                }

                int details = message.ReadUnsignedByte();
                for (int j = 0; j < details; j++) {
                    string name = message.ReadString();
                    string description = message.ReadString();
                }
            }
        }
    }
}
