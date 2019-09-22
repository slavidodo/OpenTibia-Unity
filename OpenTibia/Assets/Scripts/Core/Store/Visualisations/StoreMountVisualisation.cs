namespace OpenTibiaUnity.Core.Store.Visualisations
{
    public class StoreMountVisualisation : StoreVisualisation
    {
        private ushort _outfitId;

        public ushort OutfitId { get => _outfitId; }

        public StoreMountVisualisation(ushort outfitId) {
            _outfitId = outfitId;
        }
    }
}
