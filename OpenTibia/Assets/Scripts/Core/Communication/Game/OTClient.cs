namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        private void ParseOtclientExtendedOpcode(Internal.ByteArray message) {
            message.ReadUnsignedByte();
            message.ReadString();
        }
    }

}