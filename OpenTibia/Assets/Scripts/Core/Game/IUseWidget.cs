namespace OpenTibiaUnity.Core.Game
{
    public interface IUseWidget
    {
        int GetTopObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance @object);
        int GetUseObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance @object);
        int GetMultiUseObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance @object);
    }
}
