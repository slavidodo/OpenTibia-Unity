namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseOtclientExtendedOpcode(InputMessage message) {
            message.GetU8();
            message.GetString();
        }
    }

}