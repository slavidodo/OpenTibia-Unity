namespace OpenTibiaUnity.Core.Store.Visualisations
{
    public sealed class StoreOutfitVisualisation : StoreVisualisation
    {
        private ushort _outfitId;
        private byte _head;
        private byte _body;
        private byte _legs;
        private byte _feet;

        public ushort OutfitId { get => _outfitId; }
        public byte Head { get => _head; }
        public byte Body { get => _body; }
        public byte Legs { get => _legs; }
        public byte Feet { get => _feet; }

        public StoreOutfitVisualisation(ushort outfitId, byte head, byte body, byte legs, byte feet) {
            _outfitId = outfitId;
            _head = head;
            _body = body;
            _legs = legs;
            _feet = feet;
        }
    }
}
