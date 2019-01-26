using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class LookActionImpl : IActionImpl
    {
        Vector3Int m_AbsolutePosition;
        Appearances.AppearanceType m_AppearanceType;
        int m_StackPosition;

        public LookActionImpl(Vector3Int absolutePosition, Appearances.ObjectInstance objectInstance, int stackPosition) {
            Init(absolutePosition, objectInstance?.Type, stackPosition);
        }

        public LookActionImpl(Vector3Int absolutePosition, Appearances.AppearanceType appearnceType, int stackPosition) {
            Init(absolutePosition, appearnceType, stackPosition);
        }

        public LookActionImpl(Vector3Int absolutePosition, uint objectID, int stackPosition) {
            var appearnceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectID);
            Init(absolutePosition, appearnceType, stackPosition);
        }

        protected void Init(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPosition) {
            m_AppearanceType = appearanceType;
            if (!m_AppearanceType)
                throw new System.ArgumentException("LookActionImpl.LookActionImpl: Invalid type: " + appearanceType);

            m_AbsolutePosition = absolutePosition;
            m_StackPosition = stackPosition;
        }

        public void Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendLook(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosition);
        }
    }
}
