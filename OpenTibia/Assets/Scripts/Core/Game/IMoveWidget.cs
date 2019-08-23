namespace OpenTibiaUnity.Core.Game
{
    public interface IMoveWidget
    {
        int GetMoveObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance @object);
    }
}
