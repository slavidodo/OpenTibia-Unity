namespace OpenTibiaUnity.Core.Game
{
    public interface IUseWidget
    {
        Appearances.ObjectInstance GetUseObjectUnderPoint(UnityEngine.Vector2 point);
        Appearances.ObjectInstance GetMultiUseObjectUnderPoint(UnityEngine.Vector2 point);
    }
}
