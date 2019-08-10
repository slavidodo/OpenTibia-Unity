namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class ChatDeleteNext : StaticAction
    {
        internal ChatDeleteNext(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField && inputField.stringPosition < inputField.text.Length) {
                int startPosition = inputField.selectionStringAnchorPosition;
                int endPosition = inputField.selectionStringFocusPosition;
                if (startPosition > endPosition) {
                    int tmpPosition = startPosition;
                    startPosition = endPosition;
                    endPosition = tmpPosition;
                } else if (startPosition == endPosition) {
                    if (endPosition == inputField.text.Length)
                        return true;
                    
                    endPosition++;
                }
                
                inputField.text = inputField.text.Remove(startPosition, endPosition - startPosition);
                inputField.selectionStringFocusPosition = inputField.selectionStringAnchorPosition = startPosition;
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatDeleteNext(m_ID, m_Label, m_EventMask);
        }
    }
}
