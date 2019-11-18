using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseBuddyAdd(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            string name = message.ReadString();
            string desc = string.Empty;
            uint icon = 0;
            bool notifyLogin = false;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAdditionalVipInfo)) {
                desc = message.ReadString();
                icon = message.ReadUnsignedInt();
                notifyLogin = message.ReadBoolean();
            }

            byte status = message.ReadUnsignedByte();
            List<byte> groups;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameBuddyGroups)) {
                int count = message.ReadUnsignedByte();
                groups = new List<byte>(count);
                for (int i = 0; i < count; i++) {
                    groups.Add(message.ReadUnsignedByte());
                }
            }
        }

        private void ParseBuddyState(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
            byte state = 1;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPending))
                state = message.ReadUnsignedByte();
        }

        private void ParseBuddyLogout(Internal.CommunicationStream message) {
            uint creatureId = message.ReadUnsignedInt();
        }

        private void ParseBuddyGroupData(Internal.CommunicationStream message) {
            int groups = message.ReadUnsignedByte();
            for (int i = 0; i < groups; i++) {
                message.ReadUnsignedByte(); // id
                message.ReadString(); // name
                message.ReadUnsignedByte(); // idk
            }

            message.ReadUnsignedByte(); // premium/free iirc (since free players are allowed only for 5 groups)
        }
    }
}
