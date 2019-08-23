namespace OpenTibiaUnity.Core.DailyReward.Types
{
    public class Object : Item
    {
        ushort _id;
        string _name;
        uint _weight;
        int _amount;

        public Object(ushort id, string name, uint weight, int amount) {
            _id = id;
            _name = name;
            _weight = weight;
            _amount = amount;
        }
    }
}
