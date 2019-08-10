namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class TalkActionImpl : IActionImpl
    {
        protected bool m_AutoSend;
        private int m_ChannelID;
        protected string m_Text;

        public TalkActionImpl(string text, bool autoSend, int channelID = -1) {
            m_Text = text;
            m_AutoSend = autoSend;
            m_ChannelID = channelID;
        }

        public virtual void Perform(bool repeat = false) {
            OpenTibiaUnity.GameManager.onRequestChatSend.Invoke(m_Text, m_AutoSend, m_ChannelID);
        }
    }
}
