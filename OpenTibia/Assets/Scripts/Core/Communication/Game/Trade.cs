using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol {
        private void ParseNPCOffer(Internal.ByteArray message) {
            // todo, i believe tibia added extra data to detect currency

            string npcName = null;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNameOnNpcTrade))
                npcName = message.ReadString();

            var buyObjects = new List<Trade.TradeObjectRef>();
            var sellObjects = new List<Trade.TradeObjectRef>();

            int listCount;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 900)
                listCount = message.ReadUnsignedShort();
            else
                listCount = message.ReadUnsignedByte();

            for (int i = 0; i < listCount; i++) {
                ushort objectID = message.ReadUnsignedShort();
                ushort objectData = message.ReadUnsignedByte();

                string name = message.ReadString();
                uint weight = message.ReadUnsignedInt();
                uint buyPrice = message.ReadUnsignedInt();
                uint sellPrice = message.ReadUnsignedInt();

                if (buyPrice > 0)
                    buyObjects.Add(new Trade.TradeObjectRef(objectID, objectData, name, buyPrice, weight));

                if (sellPrice > 0)
                    sellObjects.Add(new Trade.TradeObjectRef(objectID, objectData, name, sellPrice, weight));
            }

            OpenTibiaUnity.GameManager.onRequestNPCTrade.Invoke(npcName, buyObjects, sellObjects);
        }
        private void ParsePlayerGoods(Internal.ByteArray message) {
            long money;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 973)
                money = message.ReadLong();
            else
                money = message.ReadInt();

            var goods = new List<Container.InventoryTypeInfo>();

            int size = message.ReadUnsignedByte();
            for (int i = 0; i < size; i++) {
                ushort objectID = message.ReadUnsignedShort();
                int amount;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDoubleShopSellAmount))
                    amount = message.ReadUnsignedShort();
                else
                    amount = message.ReadUnsignedByte();

                goods.Add(new Container.InventoryTypeInfo(objectID, 0, amount));
            }

            OpenTibiaUnity.ContainerStorage.PlayerGoods = goods;
            OpenTibiaUnity.ContainerStorage.PlayerMoney = money;
        }
        private void ParseCloseNPCTrade(Internal.ByteArray message) {
            OpenTibiaUnity.GameManager.onRequestCloseNPCTrade.Invoke();
        }

        private void ParseOwnOffer(Internal.ByteArray message) {
            string creatureName = message.ReadString();
            var objects = new List<Appearances.ObjectInstance>();

            int size = message.ReadUnsignedByte();
            for (int i = 0; i < size; i++)
                objects.Add(ReadObjectInstance(message));

            OpenTibiaUnity.GameManager.onRequestOwnOffer.Invoke(creatureName, objects);
        }
        private void ParseCounterOffer(Internal.ByteArray message) {
            string creatureName = message.ReadString();
            var objects = new List<Appearances.ObjectInstance>();

            int size = message.ReadUnsignedByte();
            for (int i = 0; i < size; i++)
                objects.Add(ReadObjectInstance(message));

            OpenTibiaUnity.GameManager.onRequestCounterOffer.Invoke(creatureName, objects);
        }
        private void ParseCloseTrade(Internal.ByteArray message) {
            OpenTibiaUnity.GameManager.onRequestCloseTrade.Invoke();
        }
    }
}