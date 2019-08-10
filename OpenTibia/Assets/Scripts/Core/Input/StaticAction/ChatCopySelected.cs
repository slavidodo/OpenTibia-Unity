using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class ChatCopySelected : StaticAction
    {
        internal ChatCopySelected(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!inputField)
                return false;

            var selectedString = StringHelper.GetSelection(inputField);
            if (selectedString == null)
                return false;

            GUIUtility.systemCopyBuffer = selectedString;
            return true;
        }

        public override IAction Clone() {
            return new ChatCopySelected(m_ID, m_Label, m_EventMask);
        }
    }
}
