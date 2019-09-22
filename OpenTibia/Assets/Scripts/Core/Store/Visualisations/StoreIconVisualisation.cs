namespace OpenTibiaUnity.Core.Store.Visualisations
{
    public sealed class StoreIconVisualisation : StoreVisualisation
    {
        string _icon;

        public string Icon { get => _icon; }

        public StoreIconVisualisation(string icon) {
            _icon = icon;
        }
    }
}
