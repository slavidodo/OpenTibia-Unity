using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class AutowalkActionImpl : IActionImpl
    {
        Vector3Int m_Destination;
        bool m_ForceDiagonal;
        bool m_ForceExact;

        public AutowalkActionImpl(Vector3Int absolutePosition, bool diagonal, bool exact) {
            m_Destination = absolutePosition;
            m_ForceDiagonal = diagonal;
            m_ForceExact = exact;
        }

        public void Perform(bool _ = false) {
            OpenTibiaUnity.Player.StartAutowalk(m_Destination, m_ForceDiagonal, m_ForceExact);
        }
    }
}
