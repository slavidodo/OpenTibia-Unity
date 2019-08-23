namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceTypeInfo : AppearanceTypeRef
    {
        public string Name = null;

        public AppearanceTypeInfo(ushort id, int data, string name) : base(id, data) {
            Name = name ?? throw new System.ArgumentNullException("AppearanceTypeInfo.AppearanceTypeInfo: Invalid name.");
        }
    }
}
