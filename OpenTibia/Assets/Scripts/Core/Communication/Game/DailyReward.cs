namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseRestingAreaState(Internal.ByteArray message) {
            message.ReadBoolean(); // unknown
            message.ReadBoolean(); // unknown
            message.ReadString();
        }

        private void ParseDailyRewardCollectionState(Internal.ByteArray message) {
            message.ReadUnsignedInt(); // collection tokens
        }

        private void ParseOpenRewardWall(Internal.ByteArray message) {
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

        private void ParseCloseRewardWall(Internal.ByteArray message) {

        }

        private void ParseDailyRewardBasic(Internal.ByteArray message) {
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

        private void ParseDailyRewardHistory(Internal.ByteArray message) {
            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                var timestamp = message.ReadUnsignedInt(); // timestamp
                message.ReadUnsignedByte(); // some state (0: none, 1: active[green])
                message.ReadString(); // description
                message.ReadUnsignedShort(); // streak
            }
        }

        private DailyReward.DailyReward ReadDailyReward(Internal.ByteArray message) {
            var rewardState = message.ReadEnum<DailyRewardStates>();

            var reward = new DailyReward.DailyReward(rewardState);
            switch (rewardState) {
                case DailyRewardStates.PickedItems: {
                    reward.AllowedMaximumItems = message.ReadUnsignedByte();
                    int objectCount = message.ReadUnsignedByte();
                    
                    for (int i = 0; i < objectCount; i++) {
                        ushort object_id = message.ReadUnsignedShort();
                        string objectName = message.ReadString();
                        uint objectWeight = message.ReadUnsignedInt();

                        reward.AddItem(new DailyReward.Types.Object(object_id, objectName, objectWeight, -1));
                    }

                    break;
                }

                case DailyRewardStates.FixedItems: {
                    int itemsCount = message.ReadUnsignedByte();
                    for (int i = 0; i < itemsCount; i++) {
                        var rewardType = message.ReadEnum<DailyRewardTypes>();
                        switch (rewardType) {
                            case DailyRewardTypes.Object: {
                                ushort object_id = message.ReadUnsignedShort();
                                string objectName = message.ReadString();
                                int objectAmount = message.ReadUnsignedByte();

                                reward.AddItem(new DailyReward.Types.Object(object_id, objectName, 0, objectAmount));
                                break;
                            }

                            case DailyRewardTypes.PreyBonusRerolls: {
                                int amount = message.ReadUnsignedByte();
                                reward.AddItem(new DailyReward.Types.PreyBonusRerolls(amount));
                                break;
                            }

                            case DailyRewardTypes.FiftyPercentXpBoost: {
                                ushort minutes = message.ReadUnsignedShort();
                                reward.AddItem(new DailyReward.Types.XpBoost(minutes));
                                break;
                            }

                            default:
                                throw new System.Exception("ProtocolGame.ReadDailyReward: Invalid reward type " + (int)rewardType + ".");
                        }
                    }

                    break;
                }
            }

            return reward;
        }
    }
}
