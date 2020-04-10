
namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseClientCheck(Internal.CommunicationStream message) {
            // TODO
            uint something = message.ReadUnsignedInt();
            message.Skip((int)something);
        }

        private void ParseGameNews(Internal.CommunicationStream message) {
            uint timestamp = message.ReadUnsignedInt();
            bool read = message.ReadBoolean();
        }

        private void ParseImpactTracking(Internal.CommunicationStream message) {
            bool healing = message.ReadBoolean();
            uint damage = message.ReadUnsignedInt();
        }

        private void ParseItemWasted(Internal.CommunicationStream message) {
            ushort objectId = message.ReadUnsignedShort();
        }

        private void ParseItemLooted(Internal.CommunicationStream message) {
            var @object = ProtocolGameExtentions.ReadObjectInstance(message);
            string name = message.ReadString();
        }

        private void ParseTrackedQuestFlags(Internal.CommunicationStream message) {
            bool full = message.ReadUnsignedByte() == 1;
            if (full) {
                int unknown2 = message.ReadUnsignedByte();
                int size = message.ReadUnsignedByte();
                for (int i = 0; i < size; i++) {
                    int missionId = message.ReadUnsignedShort();
                    string questName = message.ReadString();
                    string missionName = message.ReadString();
                    string missionDescription = message.ReadString();
                }
            } else {
                int missionId = message.ReadUnsignedShort();
                string missionName = message.ReadString();
                string missionDescription = message.ReadString();
            }
        }

        public void ParseKillTracking(Internal.CommunicationStream message) {
            string name = message.ReadString();
            var outfit = ProtocolGameExtentions.ReadCreatureOutfit(message);
            int lootCount = message.ReadUnsignedByte();
            for (int i = 0; i < lootCount; i++) {
                var @object = ProtocolGameExtentions.ReadObjectInstance(message);
            }
        }
    }
}
