using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class ChatInsertClipboard : StaticAction
    {
        public ChatInsertClipboard(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
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
                
                if (startPosition == endPosition)
                    inputField.text = inputField.text.Insert(startPosition, copied);
                else
                    inputField.text = inputField.text.Remove(startPosition, endPosition - startPosition).Insert(startPosition, copied);
                
                inputField.selectionStringFocusPosition = inputField.selectionStringAnchorPosition = startPosition + copied.Length;
                return true;
            }

            return false;
        }
    }
}
