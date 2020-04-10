namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class PlayerMount : StaticAction
    {

        public PlayerMount(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var player = OpenTibiaUnity.Player;
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                protocolGame.SendMount(player.MountOutfit == null);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new PlayerMount(_id, _label, _eventMask);
        }
    }
}
