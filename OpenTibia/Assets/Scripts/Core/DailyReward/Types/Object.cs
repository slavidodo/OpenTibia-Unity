namespace OpenTibiaUnity.Core.DailyReward.Types
{
    internal class Object : Item
    {
        ushort m_ID;
        string m_Name;
        uint m_Weight;
        int m_Amount;

        public Object(ushort id, string name, uint weight, int amount) {
            m_ID = id;
            m_Name = name;
            m_Weight = weight;
            m_Amount = amount;
        }
    }
}
