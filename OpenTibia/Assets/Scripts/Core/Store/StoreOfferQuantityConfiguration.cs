using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Store
{
    public class StoreOfferQuantityConfiguration
    {
        private List<string> _disabledReasons = new List<string>();
        private uint _offerId;
        private uint _price;
        private ushort _amount = 1;
        private StoreHighlightState _highlightState = StoreHighlightState.None;
        private StoreOfferDisableState _disabledState = StoreOfferDisableState.None;
        private bool _useTransferableCoins = false;

        // temporary sale related
        private uint _saleValidUntil = 0;
        private uint _saleBasePrice = 0;

        public List<string> DisabledReasons { get => _disabledReasons; }
        public StoreOfferDisableState DisabledState { get => _disabledState; set => _disabledState = value; }
        
        public StoreOfferQuantityConfiguration(uint offerId, uint price, ushort amount = 1, StoreHighlightState highlightState = StoreHighlightState.None, bool useTransferableCoins = false) {
            _offerId = offerId;
            _price = price;
            _amount = amount;
            _highlightState = highlightState;
            _useTransferableCoins = useTransferableCoins;
        }

        public void SetSaleParameters(uint validUntil, uint basePrice) {
            _saleValidUntil = validUntil;
            _saleBasePrice = basePrice;
        }
    }
}
