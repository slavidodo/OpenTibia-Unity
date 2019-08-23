namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class BrowseFieldActionImpl : IActionImpl
    {
        UnityEngine.Vector3Int _absolute;

        public BrowseFieldActionImpl(UnityEngine.Vector3Int absolute) {
            _absolute = absolute;
        }

        public void Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendBrowseField(_absolute);
        }
    }
}
