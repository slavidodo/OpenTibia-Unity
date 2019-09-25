namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseOtclientExtendedOpcode(Internal.CommunicationStream message) {
            message.ReadUnsignedByte();
            message.ReadString();
        }
    }

}