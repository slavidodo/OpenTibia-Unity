namespace OpenTibiaUnity.Core.Market
{
    internal class Offer {
        private OfferID m_ID;
        private MarketOfferTypes m_OfferType;
        private MarketOfferStates m_OfferState;

        private ushort m_TypeID;
        private ushort m_Amount;
        private uint m_PiecePrice;

        private string m_Character;

        internal bool isDubious = false;

        internal OfferID ID { get => m_ID; }
        internal MarketOfferTypes OfferType { get => m_OfferType; }
        internal MarketOfferStates OfferState { get => m_OfferState; }

        internal ushort TypeID { get => m_TypeID; }
        internal ushort Amount { get => m_Amount; }
        internal uint PiecePrice { get => m_PiecePrice; }

        internal string Character { get => m_Character; }

        internal Offer(OfferID offerID, MarketOfferTypes offerType, ushort typeID, ushort amount, uint piecePrice, string character, MarketOfferStates state) {
            m_ID = offerID;
            m_OfferType = offerType;
            m_TypeID = typeID;
            m_Amount = amount;
            m_PiecePrice = piecePrice;
            m_Character = character;
            m_OfferState = state;
        }

        internal bool IsLessThan(Offer other) {
            return m_ID.IsLessThan(other.ID);
        }

        internal bool IsEqual(Offer other) {
            return m_ID.IsEqual(other.ID);
        }
    }

    internal class OfferID
    {
        private ushort m_Counter = 0;
        private uint m_Timestamp = 0;

        internal ushort Counter { get => m_Counter; }
        internal uint Timestamp { get => m_Timestamp; }

        internal OfferID(ushort counter, uint timestamp) {
            m_Counter = counter;
            m_Timestamp = timestamp;
        }

        internal bool IsLessThan(OfferID other) {
            return m_Timestamp < other.Timestamp || m_Timestamp == other.Timestamp && m_Counter < other.Counter;
        }

        internal bool IsEqual(OfferID other) {
            return m_Timestamp == other.Timestamp && m_Counter == other.Counter;
        }
    }
}
