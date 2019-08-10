using System.Collections.Generic;

namespace OpenTibiaUnity.Core.DailyReward
{


    internal class DailyReward
    {
        DailyRewardStates m_State;
        List<Types.Item> m_Items;

        internal int AllowedMaximumItems { get; set; }

        internal DailyReward(DailyRewardStates state) {
            m_State = state;

            switch (state) {
                case DailyRewardStates.PickedItems:
                    break;

                case DailyRewardStates.FixedItems:

                    break;

                default:
                    throw new System.Exception("DailyReward.DailyReward: Invalid reward state " + (int)state + ".");
            }

            m_Items = new List<Types.Item>();
        }

        internal void AddItem(Types.Item item) {
            m_Items.Add(item);
        }
    }
}
