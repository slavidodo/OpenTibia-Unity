namespace OpenTibiaUnity.Core.Trade
{
    internal class TradeObjectRef : Appearances.AppearanceTypeRef
    {
        internal string Name { get; set; }
        internal uint Price { get; set; }
        internal uint Weight { get; set; }
        internal uint Amount { get; set; }

        internal TradeObjectRef(int id, int data, string name, uint price, uint weight, uint amount = uint.MaxValue) : base(id, data) {
            Name = name;
            Price = price;
            Weight = weight;
            Amount = amount;
        }

        internal override Appearances.AppearanceTypeRef Clone() {
            return new TradeObjectRef(m_ID, m_Data, Name, Price, Weight, Amount);
        }
    }
}
