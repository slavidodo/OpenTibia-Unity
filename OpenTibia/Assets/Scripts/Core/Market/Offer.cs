namespace OpenTibiaUnity.Core.Market
{
    public class Offer {
        private OfferId _id;
        private MarketOfferType _offerType;
        private MarketOfferState _offerState;

        private ushort _typeId;
        private ushort _amount;
        private uint _piecePrice;

        private string _character;

        public bool isDubious = false;

        public OfferId Id { get => _id; }
        public MarketOfferType OfferType { get => _offerType; }
        public MarketOfferState OfferState { get => _offerState; }

        public ushort TypeId { get => _typeId; }
        public ushort Amount { get => _amount; }
        public uint PiecePrice { get => _piecePrice; }

        public string Character { get => _character; }

        public Offer(OfferId offerId, MarketOfferType offerType, ushort typeId, ushort amount, uint piecePrice, string character, MarketOfferState state) {
            _id = offerId;
            _offerType = offerType;
            _typeId = typeId;
            _amount = amount;
            _piecePrice = piecePrice;
            _character = character;
            _offerState = state;
        }

        public bool IsLessThan(Offer other) {
            return _id.IsLessThan(other._id);
        }

        public bool IsEqual(Offer other) {
            return _id.IsEqual(other._id);
        }
    }

    public class OfferId
    {
        private ushort _counter = 0;
        private uint _timestamp = 0;

        public ushort Counter { get => _counter; }
        public uint Timestamp { get => _timestamp; }

        public OfferId(ushort counter, uint timestamp) {
            _counter = counter;
            _timestamp = timestamp;
        }

        public bool IsLessThan(OfferId other) {
            return _timestamp < other.Timestamp || _timestamp == other.Timestamp && _counter < other.Counter;
        }

        public bool IsEqual(OfferId other) {
            return _timestamp == other.Timestamp && _counter == other.Counter;
        }
    }
}
