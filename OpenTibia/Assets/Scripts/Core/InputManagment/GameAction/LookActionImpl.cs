using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class LookActionImpl : IActionImpl
    {
        Vector3Int m_AbsolutePosition;
        Appearances.AppearanceType m_AppearanceType;
        int m_StackPos;

        public LookActionImpl(Vector3Int absolutePosition, Appearances.ObjectInstance objectInstance, int stackPos) {
            Init(absolutePosition, objectInstance?.Type, stackPos);
        }

        public LookActionImpl(Vector3Int absolutePosition, Appearances.AppearanceType appearnceType, int stackPos) {
            Init(absolutePosition, appearnceType, stackPos);
        }

        public LookActionImpl(Vector3Int absolutePosition, uint objectID, int stackPos) {
            var appearnceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectID);
            Init(absolutePosition, appearnceType, stackPos);
        }

        protected void Init(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPos) {
            m_AppearanceType = appearanceType;
            if (!m_AppearanceType)
                throw new System.ArgumentException("LookActionImpl.LookActionImpl: Invalid type: " + appearanceType);

            m_AbsolutePosition = absolutePosition;
            m_StackPos = stackPos;
        }

        public void Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendLook(m_AbsolutePosition, m_AppearanceType.ID, m_StackPos);
        }
    }
}
