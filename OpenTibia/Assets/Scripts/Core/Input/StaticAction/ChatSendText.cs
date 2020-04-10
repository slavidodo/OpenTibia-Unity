using OpenTibiaUnity.Core.Input.GameAction;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatSendText : StaticAction
    {
        public ChatSendText(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            // todo; implement a chat widget and make sure it's selected
            var gameManager = OpenTibiaUnity.GameManager;
            if (gameManager.IsGameRunning) {
                new TalkActionImpl(null, true).Perform();
                return true;
            }
            return false;
        }

        public override IAction Clone() {
            return new ChatSendText(_id, _label, _eventMask);
        }
    }
}
