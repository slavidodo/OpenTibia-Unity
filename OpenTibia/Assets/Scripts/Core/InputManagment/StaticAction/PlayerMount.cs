namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class PlayerMount : StaticAction
    {

        public PlayerMount(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var player = OpenTibiaUnity.Player;
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                protocolGame.SendMount(player.MountOutfit == null);
                return true;
            }

            return false;
        }
    }
}
