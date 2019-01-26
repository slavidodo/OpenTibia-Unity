namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseVipAdd(InputMessage message) {
            uint creatureID = message.GetU32();
            string name = message.GetString();
            string desc = message.GetString();
            uint icon = message.GetU32();
            byte notifyLogin = message.GetU8();
            byte status = message.GetU8();

            byte groups = message.GetU8();
            for (int i = 0; i < groups; i++) {
                // parse groups
            }
        }
        private void ParseVipState(InputMessage message) {
            uint creatureId = message.GetU32();
            byte status = message.GetU8();
        }
        private void ParseVipLogout(InputMessage message) {
            uint creatureId = message.GetU32();
        }
    }
}
