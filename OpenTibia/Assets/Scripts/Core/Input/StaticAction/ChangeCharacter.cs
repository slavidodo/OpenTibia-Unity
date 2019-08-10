namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class ChangeCharacter : StaticAction
    {

        internal ChangeCharacter(int id, string label, uint eventMask) : base(id, label, eventMask, false) {}

        public override bool Perform(bool repeat = false) {
            if (OpenTibiaUnity.GameManager.IsGameRunning) {
                OpenTibiaUnity.GameManager.ProcessChangeCharacter();
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChangeCharacter(m_ID, m_Label, m_EventMask);
        }
    }
}
