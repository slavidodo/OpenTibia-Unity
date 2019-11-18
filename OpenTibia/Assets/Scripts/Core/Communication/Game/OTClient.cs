namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseOtclientExtendedOpcode(Internal.CommunicationStream message) {
            message.ReadUnsignedByte();
            message.ReadString();
        }
    }

}