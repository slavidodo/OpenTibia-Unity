using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatMoveCursorEnd : StaticAction
    {
        public ChatMoveCursorEnd(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextEnd(false);
                return true;
            }

            return false;
        }

        public override bool KeyCallback(InputEvent eventMask, char _, KeyCode __, EventModifiers eventModifiers) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextEnd((eventModifiers & EventModifiers.Shift) != 0);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatMoveCursorEnd(_id, _label, _eventMask);
        }
    }
}
