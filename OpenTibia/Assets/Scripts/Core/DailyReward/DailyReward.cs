using System.Collections.Generic;

namespace OpenTibiaUnity.Core.DailyReward
{


    public class DailyReward
    {
        DailyRewardType _state;
        List<Types.Item> _items;

        public int AllowedMaximumItems { get; set; }

        public DailyReward(DailyRewardType state) {
            _state = state;
            _items = new List<Types.Item>();
        }

        public void AddItem(Types.Item item) {
            _items.Add(item);
        }
    }
}
