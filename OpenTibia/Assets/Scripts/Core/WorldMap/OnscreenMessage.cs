using System.Text.RegularExpressions;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class OnscreenMessage
    {
        private static int s_NextId = 0;

        protected int _ttl;
        protected int _id;
        protected int _visibleSince;
        protected int _speakerLevel;
        protected string _speaker;
        protected MessageModeType _mode;
        protected string _text;
        protected string _richText = null;

        public int VisibleSince { get => _visibleSince; set => _visibleSince = value; }
        public int TTL { get => _ttl; set => _ttl = value; }
        public string Text { get => _text; }
        public string RichText { get => _richText; }

        public OnscreenMessage(int statementId, string speaker, int speakerLevel, MessageModeType mode, string text) {
            if (statementId <= 0)
                _id = --s_NextId;
            else
                _id = statementId;

            _speaker = speaker;
            _speakerLevel = speakerLevel;
            _mode = mode;
            _text = text;
            _visibleSince = int.MaxValue;
            _ttl = (30 + _text.Length / 3) * 100;
        }
        
        public void FormatMessage(string text, uint textARGB, uint highlightARGB) {
            _richText = StringHelper.RichTextSpecialChars(_text);
            if (_mode == MessageModeType.NpcFrom)
                _richText = StringHelper.HighlightNpcTalk(_richText, highlightARGB);

            if (text != null)
                _richText = text + _richText;

            _richText = string.Format("<color=#{0:X6}>{1}</color>", textARGB, _richText);
        }
    }
}
