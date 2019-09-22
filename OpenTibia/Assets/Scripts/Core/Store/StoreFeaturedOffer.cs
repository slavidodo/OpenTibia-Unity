namespace OpenTibiaUnity.Core.Store
{
    public class StoreFeaturedOffer
    {
        string _icon;
        StoreOpenParameters _openParams;

        public StoreFeaturedOffer(string icon, StoreOpenParameters openParams) {
            _icon = icon;
            _openParams = openParams;
        }
    }
}
