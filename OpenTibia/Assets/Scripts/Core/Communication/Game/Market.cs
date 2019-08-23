using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseMarketStatistics(Internal.ByteArray message) {
            int count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++) {
                ushort object_id = message.ReadUnsignedShort();
                uint objectPrice = message.ReadUnsignedInt();
            }
        }

        private void ParseMarketEnter(Internal.ByteArray message) {
            ulong balance;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 981)
                balance = message.ReadUnsignedLong();
            else
                balance = message.ReadUnsignedInt();

            int vocation = -1;
            if (OpenTibiaUnity.GameManager.ClientVersion < 950)
                vocation = message.ReadUnsignedByte();

            int offers = message.ReadUnsignedByte();
            int depotCount = message.ReadUnsignedByte();
            for (int i = 0; i < depotCount; i++) {
                message.ReadUnsignedShort(); // object_id
                message.ReadUnsignedShort(); // objectCount
            }
        }

        private void ParseMarketLeave(Internal.ByteArray message) {
             
        }

        private void ParseMarketDetail(Internal.ByteArray message) {
            ushort object_id = message.ReadUnsignedShort();

            var last = MarketDetails.Weight;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameImbuing))
                last = MarketDetails.ImbuementSlots;

            Dictionary<MarketDetails, string> details = new Dictionary<MarketDetails, string>();
            for (var i = MarketDetails.First; i <= last; i++) {
                int strLen = message.ReadUnsignedShort();
                if (strLen == 0)
                    continue;

                details.Add(i, message.ReadString(strLen));
            }

            int time = DateTime.Now.Second / 1000 * Constants.SecondsPerDay;
            int ctime = time;

            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                uint transactions = message.ReadUnsignedInt();
                uint totalPrice = message.ReadUnsignedInt();
                uint maximumPrice = message.ReadUnsignedInt();
                uint minimumPrice = message.ReadUnsignedInt();

                ctime -= Constants.SecondsPerDay;
            }

            ctime = time;
            count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                uint transactions = message.ReadUnsignedInt();
                uint totalPrice = message.ReadUnsignedInt();
                uint maximumPrice = message.ReadUnsignedInt();
                uint minimumPrice = message.ReadUnsignedInt();

                ctime -= Constants.SecondsPerDay;
            }
        }

        private void ParseMarketBrowse(Internal.ByteArray message) {
            ushort var = message.ReadUnsignedShort(); // this must match the current object id

            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                ReadMarketOffer(message, MarketOfferTypes.Buy, var);
            }

            count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                ReadMarketOffer(message, MarketOfferTypes.Sell, var);
            }
        }

        private Market.Offer ReadMarketOffer(Internal.ByteArray message, MarketOfferTypes offerType, ushort var) {
            uint timestamp = message.ReadUnsignedInt();
            ushort counter = message.ReadUnsignedShort();

            ushort object_id;
            switch (var) {
                case Constants.MarketRequestOwnOffers:
                case Constants.MarketRequestOwnHistory:
                    object_id = message.ReadUnsignedShort();
                    break;
                default:
                    object_id = var;
                    break;
            }

            ushort amount = message.ReadUnsignedShort();
            uint piecePrice = message.ReadUnsignedInt();

            MarketOfferStates state = MarketOfferStates.Active;
            string character = null;

            switch (var) {
                case Constants.MarketRequestOwnOffers:
                    break;
                case Constants.MarketRequestOwnHistory:
                    state = (MarketOfferStates)message.ReadUnsignedByte();
                    break;
                default:
                    character = message.ReadString();
                    break;
            }

            return new Market.Offer(new Market.OfferId(counter, timestamp), offerType, object_id, amount, piecePrice, character, state);
        }
    }
}
