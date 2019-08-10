namespace OpenTibiaUnity.Core.Game
{
    internal interface IMoveWidget
    {
        int GetMoveObjectUnderPoint(UnityEngine.Vector3 mousePosition, out Appearances.ObjectInstance @object);
    }
}
