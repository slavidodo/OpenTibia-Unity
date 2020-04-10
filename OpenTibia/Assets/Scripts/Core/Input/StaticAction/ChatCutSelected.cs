using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatCutSelected : StaticAction
    {
        public ChatCutSelected(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!inputField)
                return false;

            var selectedString = StringHelper.CutSelection(inputField);
            if (selectedString == null)
                return false;

            GUIUtility.systemCopyBuffer = selectedString;
            return true;
        }

        public override IAction Clone() {
            return new ChatCutSelected(_id, _label, _eventMask);
        }
    }
}
