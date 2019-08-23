namespace OpenTibiaUnity.Core.Trade
{
    public class TradeObjectRef : Appearances.AppearanceTypeRef
    {
        public string Name { get; set; }
        public uint Price { get; set; }
        public uint Weight { get; set; }
        public uint Amount { get; set; }

        public TradeObjectRef(ushort id, int data, string name, uint price, uint weight, uint amount = uint.MaxValue) : base(id, data) {
            Name = name;
            Price = price;
            Weight = weight;
            Amount = amount;
        }

        public override Appearances.AppearanceTypeRef Clone() {
            return new TradeObjectRef(_id, _data, Name, Price, Weight, Amount);
        }
    }
}
