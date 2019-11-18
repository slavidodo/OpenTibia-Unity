using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Cyclopedia
{
    public class CyclopediaStorage
    {
        public enum ObjectLootValue
        {
            Golden = 0,
            Purple = 1,
            Blue = 2,
            Green = 3,
            Grey = 4,
            White = 5,
        }

        public const uint GoldCoinPrice = 1;
        public const uint PlatinumCoinPrice = 100;
        public const uint CrystalCoinPrice = 10000;

        public const uint GoldenFrameMinimumPrice = 1000000;
        public const uint PurbleFrameMinimumPrice = 100000;
        public const uint BlueFrameMinimumPrice = 10000;
        public const uint GreenFrameMinimumPrice = 1000;
        public const uint GreyFrameMinimumPrice = 1;

        public static uint GoldenColor = 0xF0F000;
        public static uint PurpleColor = 0xFF68FF;
        public static uint BlueColor = 0x20A0FF;
        public static uint GreenColor = 0x00F000;
        public static uint GreyColor = 0xAAAAAA;
        public static uint WhiteColor = 0xFFFFFF;

        private Dictionary<ushort, uint> _objectMarketAveragePrices = new Dictionary<ushort, uint>();

        public uint GetObjectColor(ushort objectId) {
            var lootValue = GetObjectLootValue(objectId);
            switch (lootValue) {
                case ObjectLootValue.Golden: return GoldenColor;
                case ObjectLootValue.Purple: return PurpleColor;
                case ObjectLootValue.Blue: return BlueColor;
                case ObjectLootValue.Green: return GreenColor;
                case ObjectLootValue.Grey: return GreyColor;
                default: return WhiteColor;
            }
        }

        public ObjectLootValue GetObjectLootValue(ushort objectId) {
            uint price = GetObjectPrice(objectId);
            if (price >= GoldenFrameMinimumPrice)
                return ObjectLootValue.Golden;
            else if (price >= PurbleFrameMinimumPrice)
                return ObjectLootValue.Purple;
            else if (price >= BlueFrameMinimumPrice)
                return ObjectLootValue.Blue;
            else if (price >= GreenFrameMinimumPrice)
                return ObjectLootValue.Green;
            else if (price >= GreyFrameMinimumPrice)
                return ObjectLootValue.Grey;
            return ObjectLootValue.White;
        }

        public uint GetObjectPrice(ushort objectId) {
            uint price = 0;
            if (OpenTibiaUnity.OptionStorage.LootValueSource == CyclopediaLootValueSource.MarketAverageValue)
                _objectMarketAveragePrices.TryGetValue(objectId, out price);

            if (price == 0)
                price = GetMaximumNpcBuyPrice(objectId);
            return price;
        }

        public uint GetMaximumNpcBuyPrice(ushort objectId) {
            if (objectId == OpenTibiaUnity.AppearanceStorage.GoldCoinId)
                return GoldCoinPrice;
            else if (objectId == OpenTibiaUnity.AppearanceStorage.PlatinumCoinId)
                return PlatinumCoinPrice;
            else if (objectId == OpenTibiaUnity.AppearanceStorage.CrystalCoinId)
                return CrystalCoinPrice;

            var appearanceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectId);
            var npcSaleData = !!appearanceType ? appearanceType.NPCSaleData : null;

            uint price = 0;
            if (npcSaleData != null) {
                foreach (var data in npcSaleData)
                    price = System.Math.Max(price, data.BuyPrice);
            }

            return price;
        }
    }
}