namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseInspectionList(Internal.CommunicationStream message) {
            bool isPlayer = message.ReadBoolean();

            int size = message.ReadUnsignedByte();
            for (int i = 0; i < size; i++) {
                var @object = ProtocolGameExtentions.ReadObjectInstance(message);
                if (isPlayer) {
                    var slot = message.ReadEnum<ClothSlots>();
                }
                
                int imbuementSlots = message.ReadUnsignedByte();
                for (int j = 0; j < imbuementSlots; j++) {
                    int imbuementId = message.ReadUnsignedByte();
                }

                int details = message.ReadUnsignedByte();
                for (int j = 0; j < details; j++) {
                    string name = message.ReadString();
                    string description = message.ReadString();
                }
            }

            if (isPlayer) {
                string playerName = message.ReadString();
                var outfit = ProtocolGameExtentions.ReadCreatureOutfit(message);

                int details = message.ReadUnsignedByte();
                for (int j = 0; j < details; j++) {
                    string name = message.ReadString();
                    string description = message.ReadString();
                }
            }
        }

        private void ParseInspectionState(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            byte state = message.ReadUnsignedByte();
        }
    }
}
