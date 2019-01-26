
namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseSetTactics(InputMessage message) {
            int attackMode = message.GetU8();
            int chaseMode = message.GetU8();
            int secureMode = message.GetU8();
            int pvpMode = message.GetU8();
            
            OpenTibiaUnity.GameManager.OnTacticsChangeEvent.Invoke(
                (CombatAttackModes)attackMode,
                (CombatChaseModes)chaseMode,
                secureMode == 1,
                (CombatPvPModes)pvpMode);
        }

        private void ParseTrackedQuestFlags(InputMessage message) {
            bool full = message.GetU8() == 1;
            if (full) {
                int unknown2 = message.GetU8();
                int size = message.GetU8();
                for (int i = 0; i < size; i++) {
                    int missionId = message.GetU16();
                    string questName = message.GetString();
                    string missionName = message.GetString();
                    string missionDescription = message.GetString();
                }
            } else {
                int missionId = message.GetU16();
                string questName = message.GetString();
                string missionName = message.GetString();
            }
        }
    }
}
