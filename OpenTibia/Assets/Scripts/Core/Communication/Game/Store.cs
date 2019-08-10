namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        private void ParseStoreButtonIndicators(Internal.ByteArray message) {
            message.ReadBoolean(); // sale on items?
            message.ReadBoolean(); // new items on store?

            // TODO
        }

        private void ParseStoreCategories() {

        }
    }
}
