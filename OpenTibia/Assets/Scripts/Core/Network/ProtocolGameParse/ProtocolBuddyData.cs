namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseVipAdd(InputMessage message) {
            uint creatureID = message.GetU32();
            string name = message.GetString();
            string desc = string.Empty;
            uint icon = 0;
            bool notifyLogin = false;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameAdditionalVipInfo)) {
                desc = message.GetString();
                icon = message.GetU32();
                notifyLogin = message.GetBool();
            }

            byte status = message.GetU8();

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1110) {
                byte groups = message.GetU8();
                for (int i = 0; i < groups; i++) {
                    // parse groups
                }
            }
        }
        private void ParseVipState(InputMessage message) {
            uint creatureID = message.GetU32();
            byte status = message.GetU8();
        }
        private void ParseVipLogout(InputMessage message) {
            uint creatureID = message.GetU32();
        }
    }
}
