using System.Text.RegularExpressions;

namespace OpenTibiaUnity.Core
{
    public static class StringHelper
    {
        public delegate uint ObjectIdHighlightDelegate(ushort objectId);

        private static Regex NpcHighlightRegex = new Regex(@"\{([^}]+)\}(?:\s+\{[^}]+\})*");
        private static Regex RichTextRegex = new Regex(@"\<[^<>]+\>");
        private static Regex LootHighlightRegex = new Regex(@"\{([^}]+)\|([^}]+)\}(?:\s+\{[^}]+\})*");

        public static string HighlightNpcTalk(string text, uint highlightColor) {
            if (text == null)
                return null;

            return NpcHighlightRegex.Replace(text, (Match match) => {
                return string.Format("<link=\"{1}\"><color=#{0:X6}>{1}</color></link>", highlightColor, match.Groups[1]);
            });
        }

        public static string HighlightLootValue(string text, ObjectIdHighlightDelegate highlightARGBFunc) {
            return NpcHighlightRegex.Replace(text, (Match match) => {
                var lootText = match.Groups[1].ToString();
                var split = lootText.Split('|');
                if (split.Length == 2 && ushort.TryParse(split[0], out ushort objectId)) {
                    var highlightARGB = highlightARGBFunc(objectId);
                    return string.Format("<color=#{0:X6}>{1}</color>", highlightARGB, split[1]);
                }
                return split.Length > 0 ? split[0] : lootText;
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

        private static string GetSelectionInternal(TMPro.TMP_InputField inputField, out int startPosition, out int endPosition) {
            startPosition = inputField.selectionStringAnchorPosition;
            endPosition = inputField.selectionStringFocusPosition;
            if (startPosition > endPosition) {
                int tmpPosition = startPosition;
                startPosition = endPosition;
                endPosition = tmpPosition;
            } else if (startPosition == endPosition) {
                return null; // no selection found //
            }

            var selectedString = inputField.text.Substring(startPosition, endPosition - startPosition);
            return Regex.Replace(selectedString, @"<[^>]*>", string.Empty);
        }

        public static string GetSelection(TMPro.TMP_InputField inputField) {
            return GetSelectionInternal(inputField, out int _, out int __);
        }

        public static string CutSelection(TMPro.TMP_InputField inputField) {
            int startPosition, endPosition;
            string selection = GetSelectionInternal(inputField, out startPosition, out endPosition);

            if (!string.IsNullOrEmpty(selection)) {
                inputField.text = inputField.text.Remove(startPosition, endPosition - startPosition);
                inputField.selectionStringFocusPosition = inputField.selectionStringAnchorPosition = startPosition;
            }

            return selection;
        }
    }
}
