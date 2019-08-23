using UnityEngine;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class AutowalkActionImpl : IActionImpl
    {
        Vector3Int _destination;
        bool _forceDiagonal;
        bool _forceExact;

        public AutowalkActionImpl(Vector3Int absolutePosition, bool diagonal, bool exact) {
            _destination = absolutePosition;
            _forceDiagonal = diagonal;
            _forceExact = exact;
        }

        public void Perform(bool _ = false) {
            OpenTibiaUnity.Player.StartAutowalk(_destination, _forceDiagonal, _forceExact);
        }
    }
}
