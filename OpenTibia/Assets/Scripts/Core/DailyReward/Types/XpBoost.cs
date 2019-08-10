namespace OpenTibiaUnity.Core.DailyReward.Types
{
    internal class XpBoost : Item
    {
        int m_Minutes;

        internal XpBoost(int minutes) {
            m_Minutes = minutes;
        }
    }
}
