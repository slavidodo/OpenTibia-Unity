namespace OpenTibiaUnity.Core.Container
{
    public class InventoryTypeInfo : Appearances.AppearanceTypeRef
    {
        public int Count { get; set; }

        public InventoryTypeInfo(ushort id, int data, int count) : base(id, data) {
            Count = count;
        }
    }
}
