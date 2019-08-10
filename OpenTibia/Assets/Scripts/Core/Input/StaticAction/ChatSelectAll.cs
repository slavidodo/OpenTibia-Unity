namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class ChatSelectAll : StaticAction
    {
        internal ChatSelectAll(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
                inputField.selectionStringAnchorPosition = 0;
                inputField.selectionStringFocusPosition = inputField.text.Length;
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatSelectAll(m_ID, m_Label, m_EventMask);
        }
    }
}
