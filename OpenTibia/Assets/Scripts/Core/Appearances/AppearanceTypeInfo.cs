namespace OpenTibiaUnity.Core.Appearances
{
    class AppearanceTypeInfo : AppearanceTypeRef
    {
        public string Name = null;

        public AppearanceTypeInfo(int id, int data, string name) : base(id, data) {
            Name = name ?? throw new System.ArgumentNullException("AppearanceTypeInfo.AppearanceTypeInfo: Invalid name.");
        }
    }
}
