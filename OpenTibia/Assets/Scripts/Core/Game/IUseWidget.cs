namespace OpenTibiaUnity.Core.Game
{
    public interface IUseWidget
    {
        int GetUseObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance obj);
        int GetMultiUseObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance obj);
    }
}
