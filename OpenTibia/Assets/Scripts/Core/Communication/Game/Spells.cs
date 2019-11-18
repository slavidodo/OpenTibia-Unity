namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseSpellDelay(Internal.CommunicationStream message) {
            byte spellId = message.ReadUnsignedByte();
            uint delay = message.ReadUnsignedInt();
        }

        private void ParseSpellGroupDelay(Internal.CommunicationStream message) {
            byte groupId = message.ReadUnsignedByte();
            uint delay = message.ReadUnsignedInt();
        }

        private void ParsaeMultiUseDelay(Internal.CommunicationStream message) {
            uint delay = message.ReadUnsignedInt();
        }
    }
}
