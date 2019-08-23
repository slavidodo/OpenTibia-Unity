namespace OpenTibiaUnity.Core.DailyReward.Types
{
    public class XpBoost : Item
    {
        int _minutes;

        public XpBoost(int minutes) {
            _minutes = minutes;
        }
    }
}
