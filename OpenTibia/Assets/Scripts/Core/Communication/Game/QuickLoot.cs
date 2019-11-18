namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseUpdateLootContainers(Internal.CommunicationStream message) {
            byte unknown = message.ReadUnsignedByte();
            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                var type = message.ReadEnum<ObjectCategory>();
                ushort objectId = message.ReadUnsignedShort();
            }
        }
    }
}
