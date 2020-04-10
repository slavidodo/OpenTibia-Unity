using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatInsertClipboard : StaticAction
    {
        public ChatInsertClipboard(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            var inputField = StaticAction.GetSelectedInputField();
            if (!!inputField && !inputField.readOnly) {
                string copied = GUIUtility.systemCopyBuffer;
                if (copied == null)
                    return true;
                int startPosition = inputField.selectionStringAnchorPosition;
                int endPosition = inputField.selectionStringFocusPosition;
                if (startPosition > endPosition) {
                    int tmpPosition = startPosition;
                    startPosition = endPosition;
                    endPosition = tmpPosition;
                }

                if (inputField.lineType == TMPro.TMP_InputField.LineType.SingleLine)
                    copied = copied.Replace("\n", "");

                if (startPosition == endPosition)
                    inputField.text = inputField.text.Insert(startPosition, copied);
                else
                    inputField.text = inputField.text.Remove(startPosition, endPosition - startPosition).Insert(startPosition, copied);
                
                inputField.selectionStringFocusPosition = inputField.selectionStringAnchorPosition = startPosition + copied.Length;
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatInsertClipboard(_id, _label, _eventMask);
        }
    }
}
