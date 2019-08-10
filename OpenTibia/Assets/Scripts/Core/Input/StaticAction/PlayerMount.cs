namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class PlayerMount : StaticAction
    {

        internal PlayerMount(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

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
            return new PlayerMount(m_ID, m_Label, m_EventMask);
        }
    }
}
