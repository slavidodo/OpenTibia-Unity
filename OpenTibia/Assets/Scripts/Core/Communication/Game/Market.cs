using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseMarketStatistics(Internal.CommunicationStream message) {
            int count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++) {
                ushort objectId = message.ReadUnsignedShort();
                uint objectPrice = message.ReadUnsignedInt();
            }
        }

        private void ParseMarketEnter(Internal.CommunicationStream message) {
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
                message.ReadUnsignedShort(); // objectId
                message.ReadUnsignedShort(); // objectCount
            }
        }

        private void ParseMarketLeave(Internal.CommunicationStream message) {
             
        }

        private void ParseMarketDetail(Internal.CommunicationStream message) {
            ushort objectId = message.ReadUnsignedShort();

            var last = MarketDetail.Weight;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameImbuing))
                last = MarketDetail.ImbuementSlots;

            Dictionary<MarketDetail, string> details = new Dictionary<MarketDetail, string>();
            for (var i = MarketDetail.First; i <= last; i++) {
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

        private void ParseMarketBrowse(Internal.CommunicationStream message) {
            ushort var = message.ReadUnsignedShort(); // this must match the current object id

            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                ReadMarketOffer(message, MarketOfferType.Buy, var);
            }

            count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                ReadMarketOffer(message, MarketOfferType.Sell, var);
            }
        }

        private Market.Offer ReadMarketOffer(Internal.CommunicationStream message, MarketOfferType offerType, ushort var) {
            uint timestamp = message.ReadUnsignedInt();
            ushort counter = message.ReadUnsignedShort();

            ushort objectId;
            switch (var) {
                case Constants.MarketRequestOwnOffers:
                case Constants.MarketRequestOwnHistory:
                    objectId = message.ReadUnsignedShort();
                    break;
                default:
                    objectId = var;
                    break;
            }

            ushort amount = message.ReadUnsignedShort();
            uint piecePrice = message.ReadUnsignedInt();

            MarketOfferState state = MarketOfferState.Active;
            string character = null;

            switch (var) {
                case Constants.MarketRequestOwnOffers:
                    break;
                case Constants.MarketRequestOwnHistory:
                    state = (MarketOfferState)message.ReadUnsignedByte();
                    break;
                default:
                    character = message.ReadString();
                    break;
            }

            return new Market.Offer(new Market.OfferId(counter, timestamp), offerType, objectId, amount, piecePrice, character, state);
        }
    }
}
