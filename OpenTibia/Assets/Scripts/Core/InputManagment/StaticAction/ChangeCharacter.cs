namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class ChangeCharacter : StaticAction
    {

        public ChangeCharacter(int id, string label, uint eventMask) : base(id, label, eventMask, false) {}

        public override bool Perform(bool repeat = false) {
            if (OpenTibiaUnity.GameManager.IsGameRunning) {
                OpenTibiaUnity.GameManager.ProcessChangeCharacter();
                return true;
            }

            return false;
        }
    }
}
