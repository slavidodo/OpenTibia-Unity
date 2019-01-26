namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class LogoutCharacter : StaticAction
    {
        public LogoutCharacter(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame) {
                protocolGame.Disconnect();
                return true;
            }

            return false;
        }
    }
}
