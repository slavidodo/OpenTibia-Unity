namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class TalkActionImpl : IActionImpl
    {
        protected bool _autoSend;
        private int _channel_id;
        protected string _text;

        public TalkActionImpl(string text, bool autoSend, int channel_id = -1) {
            _text = text;
            _autoSend = autoSend;
            _channel_id = channel_id;
        }

        public virtual void Perform(bool repeat = false) {
            OpenTibiaUnity.GameManager.onRequestChatSend.Invoke(_text, _autoSend, _channel_id);
        }
    }
}
