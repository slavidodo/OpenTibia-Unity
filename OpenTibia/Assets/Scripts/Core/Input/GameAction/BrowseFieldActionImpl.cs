namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class BrowseFieldActionImpl : IActionImpl
    {
        UnityEngine.Vector3Int m_Absolute;

        public BrowseFieldActionImpl(UnityEngine.Vector3Int absolute) {
            m_Absolute = absolute;
        }

        public void Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendBrowseField(m_Absolute);
        }
    }
}
