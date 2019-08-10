using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    internal class ChannelMessage
    {
        private static int s_NextID = 0;

        private int m_ID;
        private string m_Speaker;
        private int m_SpeakerLevel;
        private MessageModeType m_Mode;
        private string m_RawText;
        private string m_RichText = null;
        private string m_PlainText = null;
        private long m_TimeStamp;
        private int m_AllowedReportTypes = 0;

        internal int ID { get => m_ID; }
        internal MessageModeType Mode { get => m_Mode; }
        internal string RawText { get => m_RawText; }
        internal string RichText { get => m_RichText; }
        internal string PlainText { get => m_PlainText; }
        internal string ReportableText { get => StringHelper.RemoveNpcHighlight(m_RawText); }
        internal string Speaker { get => m_Speaker; }
        internal int SpeakerLevel { get => m_SpeakerLevel; }
        internal long TimeStamp { get => m_TimeStamp; }

        internal ChannelMessage(int id, string speaker, int speakerLevel, MessageModeType mode, string text) {
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

        internal void SetReportTypeAllowed(ReportTypes reportType, bool add = true) {
            if (add)
                m_AllowedReportTypes |= 1 << (int)reportType;
            else
                m_AllowedReportTypes &= ~(1 << (int)reportType);
        }

        internal void FormatMessage(bool timestamps, bool levels, uint textARGB, uint highlightARGB) {
            string prefix = "";
            // todo; verify timestamps are on the same update as levels
            timestamps = timestamps && OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel);
            if (timestamps)
                prefix += string.Format("{0:HH:mm}", new System.DateTime(m_TimeStamp));

            if (m_Speaker != null) {
                levels = levels && OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel);
                if (levels && m_SpeakerLevel > 0)
                    prefix += string.Format(" {0} [{1}]", m_Speaker, m_SpeakerLevel);
                else
                    prefix += string.Format(" {0}", m_Speaker);
            }

            if (prefix.Length > 0) {
                if (m_Mode == MessageModeType.BarkLoud)
                    prefix += " ";
                else
                    prefix += ": ";
            }

            var rawText = StringHelper.RichTextSpecialChars(m_RawText);
            if (m_Mode == MessageModeType.NpcFrom || m_Mode == MessageModeType.NpcFromStartBlock)
                rawText = StringHelper.HighlightNpcTalk(rawText, highlightARGB & 16777215);

            m_RichText = string.Format("<color=#{0:X6}>{1}{2}</color>", textARGB, prefix, rawText);

            rawText = m_RawText;
            if (m_Mode == MessageModeType.NpcFrom || m_Mode == MessageModeType.NpcFromStartBlock)
                rawText = StringHelper.RemoveNpcHighlight(rawText);

            m_PlainText = rawText;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ChannelMessage);
        }
        
        public override int GetHashCode() {
            var hashCode = -517861292;
            hashCode = hashCode * -1521134295 + m_ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_Speaker);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_RawText);
            hashCode = hashCode * -1521134295 + m_SpeakerLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Mode.GetHashCode();
            hashCode = hashCode * -1521134295 + m_TimeStamp.GetHashCode();
            return hashCode;
        }
    }
}
