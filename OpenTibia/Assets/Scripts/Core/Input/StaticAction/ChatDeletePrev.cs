using UnityEngine;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatDeletePrev : StaticAction
    {
        public ChatDeletePrev(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

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
                    if (startPosition == 0) // nothing to remove
                        return true;

                    startPosition--;
                }
                
                inputField.text = inputField.text.Remove(startPosition, endPosition - startPosition);
                inputField.selectionStringFocusPosition = inputField.selectionStringAnchorPosition = startPosition;
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new ChatDeletePrev(_id, _label, _eventMask);
        }
    }
}
