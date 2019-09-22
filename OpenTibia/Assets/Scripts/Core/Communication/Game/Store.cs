using OpenTibiaUnity.Core.Store;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseStoreButtonIndicators(Internal.ByteArray message) {
            message.ReadBoolean(); // sale on items?
            message.ReadBoolean(); // new items on store?

            // TODO
        }

        private void ParseCreditBalance(Internal.ByteArray message) {
            bool updating = message.ReadBoolean();
            if (updating) {
                uint coins = message.ReadUnsignedInt();
                uint transferableCoins = message.ReadUnsignedInt();
            }
        }
        
        private void ParseStoreCategories(Internal.ByteArray message) {
            if (OpenTibiaUnity.GameManager.ClientVersion < 1180)
                ParseCreditBalance(message);

            OpenTibiaUnity.StoreManager.ClearCategories();

            int totalCategories = message.ReadUnsignedShort();
            for (int i = 0; i < totalCategories; i++) {
                var storeCategory = ReadStoreCategory(message);
                string parentCategoryName = message.ReadString();

                if (parentCategoryName.Length != 0) {
                    var parentCategory = OpenTibiaUnity.StoreManager.FindCategory(parentCategoryName);
                    if (parentCategory != null)
                        parentCategory.AddSubCategory(storeCategory);
                } else {
                    OpenTibiaUnity.StoreManager.AddCategory(storeCategory);
                }
            }
        }

        private void ParseStoreOffers(Internal.ByteArray message) {
            var gameManager = OpenTibiaUnity.GameManager;
            string categoryName = message.ReadString();

            if (gameManager.ClientVersion >= 1180) {
                uint selectedOfferId = message.ReadUnsignedInt();

                if (gameManager.ClientVersion >= 1185) {
                    message.ReadUnsignedInt();
                } else {
                    message.ReadUnsignedShort();
                }
            }

            var storeCategory = OpenTibiaUnity.StoreManager.FindCategory(categoryName);

            int offerCount = message.ReadUnsignedShort();
            for (int i = 0; i < offerCount; i++)
                storeCategory.AddOffer(ReadStoreOffer(message));

            if (gameManager.ClientVersion >= 1180 && categoryName == Constants.StoreHomeCategoryName) {
                byte featuredOfferCount = message.ReadUnsignedByte();
                for (int i = 0; i < featuredOfferCount; i++) {
                    var storeFeaturedOffer = ReadStoreFeaturedOffer(message);
                }
            }
        }

        private StoreCategory ReadStoreCategory(Internal.ByteArray message) {
            string name;
            string description = null;
            StoreHighlightState highlightState = StoreHighlightState.None;

            name = message.ReadString();
            if (OpenTibiaUnity.GameManager.ClientVersion < 1180)
                description = message.ReadString();

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStoreHighlights))
                highlightState = message.ReadEnum<StoreHighlightState>();

            var category = new StoreCategory(name, description, highlightState);

            int iconCount = message.ReadUnsignedByte();
            for (int i = 0; i < iconCount; i++)
                category.AddIcon(message.ReadString());
            
            return category;
        }

        private StoreOffer ReadStoreOffer(Internal.ByteArray message) {
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1180)
                return ReadExtendedStoreOffer(message);
            else
                return ReadLegacyStoreOffer(message);
        }

        public StoreOffer ReadExtendedStoreOffer(Internal.ByteArray message) {
            string name = message.ReadString();
            var storeOffer = new StoreOffer(name, null);

            byte quantityCount = message.ReadUnsignedByte();
            for (int i = 0; i < quantityCount; i++)
                storeOffer.AddQuantityConfiguration(ReadStoreOfferQuantityConfiguration(message));

            storeOffer.AddVisualisation(ReadStoreVisualisation(message));
            storeOffer.Filter = message.ReadString(); // unknown, tested and so far no effect.

            uint timeAddedToStore = message.ReadUnsignedInt();
            ushort timesBought = message.ReadUnsignedShort();
            bool requiresConfiguration = message.ReadBoolean();

            ushort productCount = message.ReadUnsignedShort();
            for (int i = 0; i < productCount; i++)
                storeOffer.AddProduct(ReadStoreProduct(message));
            return storeOffer;
        }

        public StoreOffer ReadLegacyStoreOffer(Internal.ByteArray message) {
            bool supportsHighlighting = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStoreHighlights);

            uint offerId = message.ReadUnsignedInt();
            string offerName = message.ReadString();
            string offerDescription = message.ReadString();
            var storeOffer = new StoreOffer(offerName, offerDescription);

            uint price = message.ReadUnsignedInt();
            var highlightState = message.ReadEnum<StoreHighlightState>();
            uint saleValidUntil = 0, saleBasePrice = 0;
            if (highlightState == StoreHighlightState.Sale && supportsHighlighting && OpenTibiaUnity.GameManager.ClientVersion >= 1097) {
                saleValidUntil = message.ReadUnsignedInt();
                saleBasePrice = message.ReadUnsignedInt();
            }

            var disabledState = message.ReadEnum<StoreOfferDisableState>();
            string disabledReason = string.Empty;
            if (supportsHighlighting && disabledState == StoreOfferDisableState.Disabled)
                disabledReason = message.ReadString();

            var quantityConfiguration = new StoreOfferQuantityConfiguration(offerId, price, 1, highlightState, false);

            quantityConfiguration.DisabledState = disabledState;
            if (disabledState == StoreOfferDisableState.Disabled)
                quantityConfiguration.DisabledReasons.Add(disabledReason);

            if (highlightState == StoreHighlightState.Sale && supportsHighlighting && OpenTibiaUnity.GameManager.ClientVersion >= 1097)
                quantityConfiguration.SetSaleParameters(saleValidUntil, saleBasePrice);

            storeOffer.AddQuantityConfiguration(quantityConfiguration);

            int iconCount = message.ReadUnsignedByte();
            for (int i = 0; i < iconCount; i++)
                storeOffer.AddVisualisation(new Store.Visualisations.StoreIconVisualisation(message.ReadString()));

            int productCount = message.ReadUnsignedShort();
            for (int i = 0; i < productCount; i++)
                storeOffer.AddProduct(ReadStoreProduct(message));

            return storeOffer;
        }

        public StoreOfferQuantityConfiguration ReadStoreOfferQuantityConfiguration(Internal.ByteArray message) {
            uint offerId = message.ReadUnsignedInt();
            ushort amount = message.ReadUnsignedShort();
            uint price = message.ReadUnsignedInt();
            bool useTransferableCoins = message.ReadBoolean();

            bool disabled = message.ReadBoolean();
            List<string> disabledReasons = new List<string>();
            if (disabled) {
                int errorCount = message.ReadUnsignedByte();
                for (int i = 0; i < errorCount; i++)
                    disabledReasons.Add(message.ReadString());
            }

            var highlightState = message.ReadEnum<StoreHighlightState>();
            if (highlightState == StoreHighlightState.Sale) {
                uint saleValidUntilTimestamp = message.ReadUnsignedInt();
                uint basePrice = message.ReadUnsignedInt();
            }

            var quantityConfiguration = new StoreOfferQuantityConfiguration(offerId, price, amount, highlightState, useTransferableCoins);
            quantityConfiguration.DisabledReasons.AddRange(disabledReasons);

            if (disabled)
                quantityConfiguration.DisabledState = StoreOfferDisableState.Disabled;

            return quantityConfiguration;
        }

        public StoreVisualisation ReadStoreVisualisation(Internal.ByteArray message) {
            var appearanceType = message.ReadEnum<StoreOfferAppearanceType>();
            switch (appearanceType) {
                case StoreOfferAppearanceType.Icon: {
                    string icon = message.ReadString();
                    return new Store.Visualisations.StoreIconVisualisation(icon);
                }
                case StoreOfferAppearanceType.Mount: {
                    ushort outfitId = message.ReadUnsignedShort();
                    return new Store.Visualisations.StoreMountVisualisation(outfitId);
                }
                case StoreOfferAppearanceType.Outfit: {
                    ushort outfitId = message.ReadUnsignedShort();
                    byte head = message.ReadUnsignedByte();
                    byte body = message.ReadUnsignedByte();
                    byte legs = message.ReadUnsignedByte();
                    byte feet = message.ReadUnsignedByte();
                    return new Store.Visualisations.StoreOutfitVisualisation(outfitId, head, body, legs, feet);
                }
                case StoreOfferAppearanceType.Object: {
                    ushort objectId = message.ReadUnsignedShort();
                    return new Store.Visualisations.StoreObjectVisualisation(objectId);
                }
            }

            return null;
        }

        public StoreProduct ReadStoreProduct(Internal.ByteArray message) {
            string name = message.ReadString();
            string description = null;
            List<StoreVisualisation> visualisations = new List<StoreVisualisation>();
            if (OpenTibiaUnity.GameManager.ClientVersion < 1180) {
                description = message.ReadString();

                int iconCount = message.ReadUnsignedByte();
                for (int i = 0; i < iconCount; i++)
                    visualisations.Add(new Store.Visualisations.StoreIconVisualisation(message.ReadString()));
            } else {
                visualisations.Add(ReadStoreVisualisation(message));
            }
            
            return new StoreProduct(name, description, visualisations);
        }

        public StoreFeaturedOffer ReadStoreFeaturedOffer(Internal.ByteArray message) {
            string icon = message.ReadString();
            var openParameters = ReadStoreOpenParameters(message);
            return new StoreFeaturedOffer(icon, openParameters);
        }

        public StoreOpenParameters ReadStoreOpenParameters(Internal.ByteArray message) {
            var openAction = message.ReadEnum<StoreOpenParameterAction>();

            Store.OpenParameters.IStoreOpenParamater openParam = null;
            switch (openAction) {
                case StoreOpenParameterAction.Invalid: {
                    break;
                }
                case StoreOpenParameterAction.CategoryType: {
                    var categoryType = message.ReadEnum<StoreCategoryType>();
                    openParam = new Store.OpenParameters.StoreCategoryTypeOpenParamater(categoryType);
                    break;
                }
                case StoreOpenParameterAction.CategoryAndFilter: {
                    var categoryAndFilter = ReadStoreCategoryAndFilter(message);
                    openParam = new Store.OpenParameters.StoreCategoryAndFilterOpenParamater(categoryAndFilter);
                    break;
                }
                case StoreOpenParameterAction.OfferType: {
                    var offerType = message.ReadEnum<StoreOfferType>();
                    openParam = new Store.OpenParameters.StoreOfferTypeOpenParamater(offerType);
                    break;
                }
                case StoreOpenParameterAction.OfferId: {
                    var offerId = message.ReadUnsignedInt();
                    openParam = new Store.OpenParameters.StoreOfferIdOpenParamater(offerId);
                    break;
                }
                case StoreOpenParameterAction.CategoryName: {
                    var categoryName = message.ReadString();
                    openParam = new Store.OpenParameters.StoreCategoryNameOpenParamater(categoryName);
                    break;
                }
            }
            
            // enum too, 0, 1, 2, 3
            message.ReadUnsignedByte(); 

            /**
             * 0: default
             * 1: home
             * // 2, 3?
             */
            message.ReadBoolean(); // 0, 1, 2, 3 (enum)
            return new StoreOpenParameters(openAction, openParam);
        }

        public StoreCategoryAndFilter ReadStoreCategoryAndFilter(Internal.ByteArray message) {
            string category = message.ReadString();
            string filter = message.ReadString();
            return new StoreCategoryAndFilter(category, filter);
        }
    }
}
