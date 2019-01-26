using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class ChatMoveCursorHome : StaticAction
    {
        public ChatMoveCursorHome(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextStart(false);
                return true;
            }

            return false;
        }

        public override bool KeyCallback(uint eventMask, char _, KeyCode __, EventModifiers eventModifiers) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextStart((eventModifiers & EventModifiers.Shift) != 0);
                return true;
            }

            return false;
        }
    }
}
