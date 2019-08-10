namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class LogoutCharacter : StaticAction
    {
        internal LogoutCharacter(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame) {
                protocolGame.Disconnect(false);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new LogoutCharacter(m_ID, m_Label, m_EventMask);
        }
    }
}
