namespace OpenTibiaUnity.Core.Chat
{
    public class ChannelMessage
    {
        private static int s_NextID = 0;

        private int m_ID;
        private string m_Speaker;
        private int m_SpeakerLevel;
        private MessageModes m_Mode;
        private string m_RawText;
        private string m_RichText = null;
        private string m_PlainText = null;
        private long m_TimeStamp;
        private int m_AllowedReportTypes = 0;

        public int ID {
            get { return m_ID; }
        }
        
        public MessageModes Mode {
            get { return m_Mode; }
        }

        public string RichText {
            get { return m_RichText; }
        }

        public string PlainText {
            get { return m_PlainText; }
        }

        public string ReportableText {
            get { return StringHelper.RemoveNpcHighlight(m_RawText); }
        }

        public string Speaker {
            get { return m_Speaker; }
        }

        public ChannelMessage(int id, string speaker, int speakerLevel, MessageModes mode, string text) {
            if (id <= 0)
                m_ID = --s_NextID;
            else
                m_ID = id;

            m_Speaker = speaker;
            m_SpeakerLevel = speakerLevel;
            m_Mode = mode;
            m_RawText = text;
            m_TimeStamp = System.DateTime.Now.Ticks;
        }

        public void SetReportTypeAllowed(ReportTypes reportType, bool add = true) {
            if (add)
                m_AllowedReportTypes |= 1 << (int)reportType;
            else
                m_AllowedReportTypes &= ~(1 << (int)reportType);
        }

        public void FormatMessage(bool timestamps, bool levels, uint textARGB, uint highlightARGB) {
            string text = "";
            if (timestamps)
                text += string.Format("{0:HH:mm}", new System.DateTime(m_TimeStamp));

            if (m_Speaker != null) {
                if (levels && m_SpeakerLevel > 0)
                    text += string.Format(" {0} [{1}]", m_Speaker, m_SpeakerLevel);
                else
                    text += string.Format(" {0}", m_Speaker);
            }

            if (text.Length > 0)
                text += ":";

            var rawText = StringHelper.RichTextSpecialChars(m_RawText);
            if (m_Mode == MessageModes.NpcFrom || m_Mode == MessageModes.NpcFromStartBlock)
                rawText = StringHelper.HighlightNpcTalk(rawText, highlightARGB & 16777215);

            m_RichText = string.Format("<color=#{0:X6}>{1} {2}</color>", textARGB, text, rawText);

            rawText = m_RawText;
            if (m_Mode == MessageModes.NpcFrom || m_Mode == MessageModes.NpcFromStartBlock)
                rawText = StringHelper.RemoveNpcHighlight(rawText);

            m_PlainText = rawText;
        }
    }
}
