using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatMoveCursorRight : StaticAction
    {
        public ChatMoveCursorRight(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.caretPosition = Mathf.Min(inputField.caretPosition + 1, inputField.text.Length);
                return true;
            }

            return false;
        }

        public override bool KeyCallback(InputEvent eventMask, char _, KeyCode __, EventModifiers eventModifiers) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.caretPosition = Mathf.Min(inputField.caretPosition + 1, inputField.text.Length);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatMoveCursorRight(_id, _label, _eventMask);
        }
    }
}
