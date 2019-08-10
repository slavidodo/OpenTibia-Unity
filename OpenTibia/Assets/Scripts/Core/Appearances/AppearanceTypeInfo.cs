namespace OpenTibiaUnity.Core.Appearances
{
    internal class AppearanceTypeInfo : AppearanceTypeRef
    {
        internal string Name = null;

        internal AppearanceTypeInfo(int id, int data, string name) : base(id, data) {
            Name = name ?? throw new System.ArgumentNullException("AppearanceTypeInfo.AppearanceTypeInfo: Invalid name.");
        }
    }
}
