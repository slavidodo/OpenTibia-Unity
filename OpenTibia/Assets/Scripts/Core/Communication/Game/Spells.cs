namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseSpellDelay(Internal.ByteArray message) {
            byte spellId = message.ReadUnsignedByte();
            uint delay = message.ReadUnsignedInt();
        }

        private void ParseSpellGroupDelay(Internal.ByteArray message) {
            byte groupId = message.ReadUnsignedByte();
            uint delay = message.ReadUnsignedInt();
        }
    }
}
