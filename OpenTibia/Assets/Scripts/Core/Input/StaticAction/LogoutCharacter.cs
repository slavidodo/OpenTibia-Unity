namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class LogoutCharacter : StaticAction
    {
        public LogoutCharacter(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame) {
                protocolGame.Disconnect(false);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new LogoutCharacter(_id, _label, _eventMask);
        }
    }
}
