using UnityEngine;

namespace OpenTibiaUnity.Core.Game
{
    public interface IWidgetContainerWidget
    {
        Vector3Int? MousePositionToAbsolutePosition(Vector3 mousePosition);
        Vector3Int? MousePositionToMapPosition(Vector3 mousePosition);
    }
}
