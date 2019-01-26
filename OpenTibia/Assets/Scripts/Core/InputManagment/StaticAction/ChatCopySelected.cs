using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class ChatCopySelected : StaticAction
    {
        public ChatCopySelected(int id, string label, uint eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            TMPro.TMP_InputField inputField = StaticAction.GetSelectedInputField();
            if (!!inputField) {
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
                
                GUIUtility.systemCopyBuffer = inputField.text.Substring(startPosition, endPosition - startPosition);
                return true;
            }

            return false;
        }
    }
}
