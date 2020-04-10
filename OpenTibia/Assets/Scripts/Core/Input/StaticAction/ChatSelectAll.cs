namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatSelectAll : StaticAction
    {
        public ChatSelectAll(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

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
            return new ChatSelectAll(_id, _label, _eventMask);
        }
    }
}
