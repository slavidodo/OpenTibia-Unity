using System.Collections.Generic;

namespace OpenTibiaUnity.Core.DailyReward
{


    public class DailyReward
    {
        DailyRewardStates _state;
        List<Types.Item> _items;

        public int AllowedMaximumItems { get; set; }

        public DailyReward(DailyRewardStates state) {
            _state = state;

            switch (state) {
                case DailyRewardStates.PickedItems:
                    break;

                case DailyRewardStates.FixedItems:

                    break;

                default:
                    throw new System.Exception("DailyReward.DailyReward: Invalid reward state " + (int)state + ".");
            }

            _items = new List<Types.Item>();
        }

        public void AddItem(Types.Item item) {
            _items.Add(item);
        }
    }
}
