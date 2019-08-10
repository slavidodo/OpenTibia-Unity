using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class ChatMoveCursorEnd : StaticAction
    {
        internal ChatMoveCursorEnd(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextEnd(false);
                return true;
            }

            return false;
        }

        internal override bool KeyCallback(uint eventMask, char _, KeyCode __, EventModifiers eventModifiers) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.MoveTextEnd((eventModifiers & EventModifiers.Shift) != 0);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatMoveCursorEnd(m_ID, m_Label, m_EventMask);
        }
    }
}
