namespace OpenTibiaUnity.Core.Store.Visualisations
{
    public class StoreObjectVisualisation : StoreVisualisation
    {
        private ushort _objectId;

        public ushort ObjectId { get => _objectId; }

        public StoreObjectVisualisation(ushort objectId) {
            _objectId = objectId;
        }
    }
}
