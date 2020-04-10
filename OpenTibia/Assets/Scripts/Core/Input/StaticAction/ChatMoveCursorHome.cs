using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatMoveCursorHome : StaticAction
    {
        public ChatMoveCursorHome(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }
        
        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextStart(false);
                return true;
            }

            return false;
        }

        public override bool KeyCallback(InputEvent eventMask, char _, KeyCode __, EventModifiers eventModifiers) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextStart((eventModifiers & EventModifiers.Shift) != 0);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatMoveCursorHome(_id, _label, _eventMask);
        }
    }
}
