namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class TalkActionImpl : IActionImpl
    {
        protected bool _autoSend;
        private int _channelId;
        protected string _text;

        public TalkActionImpl(string text, bool autoSend, int channelId = -1) {
            _text = text;
            _autoSend = autoSend;
            _channelId = channelId;
        }

        public virtual void Perform(bool repeat = false) {
            OpenTibiaUnity.GameManager.onRequestChatSend.Invoke(_text, _autoSend, _channelId);
        }
    }
}
