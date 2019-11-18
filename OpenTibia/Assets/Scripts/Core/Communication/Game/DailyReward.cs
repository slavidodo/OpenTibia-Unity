namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseRestingAreaState(Internal.CommunicationStream message) {
            bool inRestingArea = message.ReadBoolean();
            bool restingBonusActive = message.ReadBoolean();
            string description = message.ReadString();
        }

        private void ParseDailyRewardCollectionState(Internal.CommunicationStream message) {
            message.ReadUnsignedByte(); // collection tokens
        }

        private void ParseOpenRewardWall(Internal.CommunicationStream message) {
            message.ReadBoolean(); // openedFromShrine
            message.ReadUnsignedInt(); // timestamp for the player to be able to take the reward (0 = able)
            message.ReadUnsignedByte(); // currentRewardIndex
            bool showWarningWhenCollecting = message.ReadUnsignedByte() != 0; // showWarningWhenCollecting
            if (showWarningWhenCollecting) {
                message.ReadString(); // warningMessage
            }

            message.ReadUnsignedInt(); // timestamp for the streak to expire (0 = won't expire until server save)
            message.ReadUnsignedShort(); // current streak
            message.ReadUnsignedShort(); // unknown
        }

        private void ParseCloseRewardWall(Internal.CommunicationStream message) {

        }

        private void ParseDailyRewardBasic(Internal.CommunicationStream message) {
            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                var freeReward = ReadDailyReward(message);
                var premiumReward = ReadDailyReward(message);
            }

            count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                string bonusName = message.ReadString();
                int streakRequired = message.ReadUnsignedByte();
            }

            message.ReadUnsignedByte(); // activeBonuses
        }

        private void ParseDailyRewardHistory(Internal.CommunicationStream message) {
            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                var timestamp = message.ReadUnsignedInt(); // timestamp
                message.ReadUnsignedByte(); // some state (0: none, 1: active[green])
                message.ReadString(); // description
                message.ReadUnsignedShort(); // streak
            }
        }

        private DailyReward.DailyReward ReadDailyReward(Internal.CommunicationStream message) {
            var rewardType = message.ReadEnum<DailyRewardType>();
            var reward = new DailyReward.DailyReward(rewardType);

            switch (rewardType) {
                case DailyRewardType.PickedItems: {
                    reward.AllowedMaximumItems = message.ReadUnsignedByte();
                    int objectCount = message.ReadUnsignedByte();
                    
                    for (int i = 0; i < objectCount; i++) {
                        ushort objectId = message.ReadUnsignedShort();
                        string objectName = message.ReadString();
                        uint objectWeight = message.ReadUnsignedInt();

                        reward.AddItem(new DailyReward.Types.Object(objectId, objectName, objectWeight, -1));
                    }

                    break;
                }

                case DailyRewardType.FixedItems: {
                    int itemsCount = message.ReadUnsignedByte();
                    for (int i = 0; i < itemsCount; i++) {
                        var subType = message.ReadEnum<DailyRewardSubType>();
                        switch (subType) {
                            case DailyRewardSubType.Object: {
                                ushort objectId = message.ReadUnsignedShort();
                                string objectName = message.ReadString();
                                int objectAmount = message.ReadUnsignedByte();

                                reward.AddItem(new DailyReward.Types.Object(objectId, objectName, 0, objectAmount));
                                break;
                            }

                            case DailyRewardSubType.PreyBonusRerolls: {
                                int amount = message.ReadUnsignedByte();
                                reward.AddItem(new DailyReward.Types.PreyBonusRerolls(amount));
                                break;
                            }

                            case DailyRewardSubType.FiftyPercentXpBoost: {
                                ushort minutes = message.ReadUnsignedShort();
                                reward.AddItem(new DailyReward.Types.XpBoost(minutes));
                                break;
                            }

                            default:
                                throw new System.Exception("ProtocolGame.ReadDailyReward: Invalid reward sub-type " + (int)subType + ".");
                        }
                    }

                    break;
                }
            }

            return reward;
        }
    }
}
