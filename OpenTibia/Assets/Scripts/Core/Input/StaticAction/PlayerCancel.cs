namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class PlayerCancel : StaticAction
    {
        public PlayerCancel(int id, string label, InputEvent eventMask) : base(id, label, eventMask) { }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var player = OpenTibiaUnity.Player;
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            if (!!protocolGame && !!player && protocolGame.IsGameRunning && creatureStorage != null) {
                player.StopAutowalk(false);
                creatureStorage.SetAttackTarget(null, false);
                protocolGame.SendCancel();
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new PlayerCancel(_id, _label, _eventMask);
        }
    }
}
