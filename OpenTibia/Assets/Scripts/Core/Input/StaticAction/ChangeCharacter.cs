namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChangeCharacter : StaticAction
    {

        public ChangeCharacter(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) {}

        public override bool Perform(bool repeat = false) {
            if (OpenTibiaUnity.GameManager.IsGameRunning) {
                OpenTibiaUnity.GameManager.ProcessChangeCharacter();
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChangeCharacter(_id, _label, _eventMask);
        }
    }
}
