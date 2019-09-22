using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Store
{
    public class StoreCategory
    {
        List<StoreCategory> _subCategories;
        List<StoreOffer> _offers;
        List<string> _icons;
        string _name;
        string _description;
        StoreHighlightState _highlightState;

        public List<StoreCategory> SubCategories { get => _subCategories; }
        public List<StoreOffer> Offers { get => _offers; }
        public List<string> Icons { get => _icons; }
        public string Name { get => _name; }
        public string Description { get => _description; }
        public StoreHighlightState HighlightState { get => _highlightState; }

        public StoreCategory(string name, string description, StoreHighlightState highlightState) {
            _name = name;
            _description = description;
            _highlightState = highlightState;

            _subCategories = new List<StoreCategory>();
            _offers = new List<StoreOffer>();
            _icons = new List<string>();
        }

        public void ClearOffers() {
            _offers.Clear();
        }
        public void AddOffer(StoreOffer offer) {
            _offers.Add(offer);
        }
        public StoreOffer GetOffer(int index) {
            return _offers[index];
        }

        public void ClearIcons() {
            _icons.Clear();
        }
        public void AddIcon(string icon) {
            _icons.Add(icon);
        }
        public string GetIcon(int index) {
            return _icons[index];
        }

        public void ClearSubCategories() {
            _subCategories.Clear();
        }
        public void AddSubCategory(StoreCategory category) {
            _subCategories.Add(category);
        }
        public StoreCategory GetSubCategory(int index) {
            return _subCategories[index];
        }
    }
}
