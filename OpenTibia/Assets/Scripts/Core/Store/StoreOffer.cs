using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Store
{
    public class StoreOffer
    {
        List<StoreOfferQuantityConfiguration> _quantityConfigurations = new List<StoreOfferQuantityConfiguration>();
        List<StoreVisualisation> _visualisations = new List<StoreVisualisation>();
        List<StoreProduct> _products = new List<StoreProduct>();
        private string _name;
        private string _description;
        private string _filter;

        public string Name { get => _name; }
        public string Description { get => _description; }
        public string Filter { get => _filter; set => _filter = value; }

        public uint TimeAddedToStore { get; set; }
        public ushort TimesBought { get; set; }
        public bool RequiresConfiguration { get; set; }

        public StoreOffer(string name) {
            _name = name;
        }

        public StoreOffer(string name, string description) : this(name) {
            _description = description;
        }

        public void AddQuantityConfiguration(StoreOfferQuantityConfiguration quantityConfiguration) {
            _quantityConfigurations.Add(quantityConfiguration);
        }

        public void AddVisualisation(StoreVisualisation visualisation) {
            _visualisations.Add(visualisation);
        }

        public void AddProduct(StoreProduct product) {
            _products.Add(product);
        }
    }
}
