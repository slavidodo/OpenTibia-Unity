using UnityEngine;

namespace OpenTibiaUnity.Core.Game
{
    public interface IWidgetContainerWidget
    {
        Vector3Int? PointToAbsolute(Vector3 mousePosition);
        Vector3Int? PointToMap(Vector3 mousePosition);
    }
}
