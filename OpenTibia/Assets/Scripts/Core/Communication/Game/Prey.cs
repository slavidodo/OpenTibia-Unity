namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParsePreyFreeListRerollAvailability(Internal.CommunicationStream message) {
            byte slot = message.ReadUnsignedByte();
            ushort minutes = message.ReadUnsignedShort();
        }

        private void ParsePreyTimeLeft(Internal.CommunicationStream message) {
            byte slot = message.ReadUnsignedByte();
            ushort minutes = message.ReadUnsignedShort();
        }

        private void ParsePreyData(Internal.CommunicationStream message) {
            int slot = message.ReadUnsignedByte();
            var state = message.ReadEnum<PreySlotStates>();
            switch (state) {
                case PreySlotStates.Locked: {
                    message.ReadEnum<PreySlotUnlockType>();
                    break;
                }

                case PreySlotStates.Inactive: {
                    break;
                }

                case PreySlotStates.Active: {
                    string monsterName = message.ReadString();
                    var monsterOutfit = ProtocolGameExtentions.ReadCreatureOutfit(message);
                    var bonusType = message.ReadEnum<PreyBonusTypes>();
                    int bonusValue = message.ReadUnsignedShort();
                    int bonusGrade = message.ReadUnsignedByte();
                    int timeLeft = message.ReadUnsignedShort();
                    break;
                }

                case PreySlotStates.Selection: {
                    byte size = message.ReadUnsignedByte();
                    for (int i = 0; i < size; i++) {
                        string monsterName = message.ReadString();
                        var monsterOutfit = ProtocolGameExtentions.ReadCreatureOutfit(message);
                    }
                    break;
                }

                case PreySlotStates.SelectionChangeMonster: {
                    var bonusType = message.ReadEnum<PreyBonusTypes>();
                    int bonusValue = message.ReadUnsignedShort();
                    int bonusGrade = message.ReadUnsignedByte();
                    byte size = message.ReadUnsignedByte();
                    for (int i = 0; i < size; i++) {
                        string monsterName = message.ReadString();
                        var monsterOutfit = ProtocolGameExtentions.ReadCreatureOutfit(message);
                    }
                    break;
                }
                default:
                    break;
            }

            message.ReadUnsignedShort(); // timeUntilFreeListReroll
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1190) {
                message.ReadUnsignedByte(); // preyWildCards
            }
        }

        private void ParsePreyPrices(Internal.CommunicationStream message) {
            message.ReadUnsignedInt(); // rerollPrice in gold
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1190) {
                message.ReadUnsignedByte(); // unknown
                message.ReadUnsignedByte(); // selectCreatureDirectly in preyWildCards
            }
        }
    }
}
