namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ChatHistoryPrev : StaticAction
    {
        public ChatHistoryPrev(int id, string label, InputEvent eventMask) : base(id, label, eventMask, false) { }

        public override bool Perform(bool repeat = false) {
            OpenTibiaUnity.GameManager.onRequestChatHistoryPrev.Invoke();
            return true;
        }

        public override IAction Clone() {
            return new ChatHistoryPrev(_id, _label, _eventMask);
        }
    }
}
