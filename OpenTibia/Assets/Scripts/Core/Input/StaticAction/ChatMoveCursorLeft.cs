using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatMoveCursorLeft : StaticAction
    {
        public ChatMoveCursorLeft(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.caretPosition = Mathf.Max(inputField.caretPosition - 1, 0);
                return true;
            }

            return false;
        }

        public override bool KeyCallback(InputEvent eventMask, char _, KeyCode __, EventModifiers eventModifiers) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.caretPosition = Mathf.Max(inputField.caretPosition - 1, 0);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatMoveCursorLeft(_id, _label, _eventMask);
        }
    }
}
