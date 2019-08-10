namespace OpenTibiaUnity.Core.Container
{
    internal class InventoryTypeInfo : Appearances.AppearanceTypeRef
    {
        internal int Count { get; set; }

        internal InventoryTypeInfo(int id, int data, int count) : base(id, data) {
            Count = count;
        }
    }
}
