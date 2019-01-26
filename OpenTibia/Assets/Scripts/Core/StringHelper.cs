using System.Text.RegularExpressions;

namespace OpenTibiaUnity.Core
{
    public static class StringHelper
    {
        private static Regex NpcHighlightRegex = new Regex(@"\{([^}]+)\}(?:\s+\{[^}]+\})*");
        private static Regex RichTextRegex = new Regex(@"\<[^<>]+\>");

        public static string HighlightNpcTalk(string text, uint highlightColor) {
            if (text == null)
                return null;

            return NpcHighlightRegex.Replace(text, (Match match) => {
                return string.Format("<color=#{0:X6}>{1}</color>", highlightColor, match.Groups[1]);
            });
        }

        public static string RemoveNpcHighlight(string text) {
            if (text == null)
                return null;

            return NpcHighlightRegex.Replace(text, (Match match) => { return match.Groups[1].ToString(); });
        }

        public static string FormatTime(System.DateTime dateTime) {
            return string.Format("{0:HH:mm}", dateTime);
        }

        public static string FormatTime(long timeStamp) {
            return FormatTime(new System.DateTime(timeStamp));
        }

        public static string RichTextSpecialChars(string text) {
            if (text == null)
                return null;

            return RichTextRegex.Replace(text, (Match m) => { return $"<noparse>{m.Value}</noparse>"; });
        }
    }
}
