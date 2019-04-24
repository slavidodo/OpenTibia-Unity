namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class ToggleWrapStateActionImpl : IActionImpl
    {

        UnityEngine.Vector3Int m_Absolute;
        Appearances.AppearanceType m_AppearanceType;
        int m_StackPos;

        public ToggleWrapStateActionImpl(UnityEngine.Vector3Int absolute, Appearances.ObjectInstance obj, int stackPos) {
            m_Absolute = absolute;
            m_AppearanceType = obj?.Type ?? throw new System.ArgumentNullException("ToggleWrapStateActionImpl.ToggleWrapStateActionImpl: Invalid object.");
            m_StackPos = stackPos;
        }

        public void Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendToggleWrapState(m_Absolute, m_AppearanceType.ID, m_StackPos);
        }
    }
}
