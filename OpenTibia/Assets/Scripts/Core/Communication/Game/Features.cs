
namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseClientCheck(Internal.ByteArray message) {
            // TODO
            uint something = message.ReadUnsignedInt();
            message.Skip((int)something);
        }

        private void ParseTrackedQuestFlags(Internal.ByteArray message) {
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
                string questName = message.ReadString();
                string missionName = message.ReadString();
            }
        }
        
        private void ParseGameNews(Internal.ByteArray message) {
            // TODO (suggested structure; timestamp, boolean)
            message.ReadUnsignedInt();
            message.ReadUnsignedByte();
        }
    }
}
