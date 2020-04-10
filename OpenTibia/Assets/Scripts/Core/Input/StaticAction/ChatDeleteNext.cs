namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatDeleteNext : StaticAction
    {
        public ChatDeleteNext(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

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
            return new ChatDeleteNext(_id, _label, _eventMask);
        }
    }
}
