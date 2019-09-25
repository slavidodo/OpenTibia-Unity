using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class ChannelMessage
    {
        private static int s_NextId = 0;

        private int _id;
        private string _speaker;
        private int _speakerLevel;
        private MessageModeType _mode;
        private string _rawText;
        private string _richText = null;
        private string _plainText = null;
        private long _timeStamp;
        private int _allowedReportTypes = 0;

        public int Id { get => _id; }
        public MessageModeType Mode { get => _mode; }
        public string RawText { get => _rawText; }
        public string RichText { get => _richText; }
        public string PlainText { get => _plainText; }
        public string ReportableText { get => StringHelper.RemoveNpcHighlight(_rawText); }
        public string Speaker { get => _speaker; }
        public int SpeakerLevel { get => _speakerLevel; }
        public long TimeStamp { get => _timeStamp; }

        public ChannelMessage(int id, string speaker, int speakerLevel, MessageModeType mode, string text) {
            if (id <= 0)
                _id = --s_NextId;
            else
                _id = id;

            _speaker = speaker;
            _speakerLevel = speakerLevel;
            _mode = mode;
            _rawText = text;
            _timeStamp = System.DateTime.Now.Ticks;
        }

        public void SetReportTypeAllowed(ReportTypes reportType, bool add = true) {
            if (add)
                _allowedReportTypes |= 1 << (int)reportType;
            else
                _allowedReportTypes &= ~(1 << (int)reportType);
        }

        public void FormatMessage(bool timestamps, bool levels, uint textARGB, uint highlightARGB) {
            string prefix = "";
            // todo; verify timestamps are on the same update as levels
            timestamps = timestamps && OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel);
            if (timestamps)
                prefix += string.Format("{0:HH:mm}", new System.DateTime(_timeStamp));

            if (_speaker != null) {
                levels = levels && OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel);
                if (levels && _speakerLevel > 0)
                    prefix += string.Format(" {0} [{1}]", _speaker, _speakerLevel);
                else
                    prefix += string.Format(" {0}", _speaker);
            }

            if (prefix.Length > 0) {
                if (_mode == MessageModeType.BarkLoud)
                    prefix += " ";
                else
                    prefix += ": ";
            }

            var rawText = StringHelper.RichTextSpecialChars(_rawText);
            if (_mode == MessageModeType.NpcFrom || _mode == MessageModeType.NpcFromStartBlock)
                rawText = StringHelper.HighlightNpcTalk(rawText, highlightARGB & 16777215);
            else if (_mode == MessageModeType.Loot && OpenTibiaUnity.GameManager.ClientVersion >= 1200)
                rawText = StringHelper.HighlightLootValue(rawText, (ushort objectId) => {
                    return OpenTibiaUnity.CyclopediaStorage.GetObjectColor(objectId);
                });
            _richText = string.Format("<color=#{0:X6}>{1}{2}</color>", textARGB, prefix, rawText);

            rawText = _rawText;
            if (_mode == MessageModeType.NpcFrom || _mode == MessageModeType.NpcFromStartBlock)
                rawText = StringHelper.RemoveNpcHighlight(rawText);

            _plainText = rawText;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ChannelMessage);
        }
        
        public override int GetHashCode() {
            var hashCode = -517861292;
            hashCode = hashCode * -1521134295 + _id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_speaker);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_rawText);
            hashCode = hashCode * -1521134295 + _speakerLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + _mode.GetHashCode();
            hashCode = hashCode * -1521134295 + _timeStamp.GetHashCode();
            return hashCode;
        }
    }
}
