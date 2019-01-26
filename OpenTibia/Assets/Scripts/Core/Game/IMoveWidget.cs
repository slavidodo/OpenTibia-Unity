namespace OpenTibiaUnity.Core.Game
{
    public interface IMoveWidget
    {
        Appearances.ObjectInstance GetMoveObjectUnderPoint(UnityEngine.Vector2 point);
    }
}
