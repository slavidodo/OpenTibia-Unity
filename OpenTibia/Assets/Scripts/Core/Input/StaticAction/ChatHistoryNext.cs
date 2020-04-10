namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatHistoryNext : StaticAction
    {
        public ChatHistoryNext(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            OpenTibiaUnity.GameManager.onRequestChatHistoryNext.Invoke();
            return true;
        }

        public override IAction Clone() {
            return new ChatHistoryNext(_id, _label, _eventMask);
        }
    }
}
