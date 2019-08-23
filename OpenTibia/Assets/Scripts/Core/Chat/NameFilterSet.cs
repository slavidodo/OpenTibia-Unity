using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class NameFilterSet
    {
        public static int DefaultSet = 0;

        private int _id;

        public bool BlacklistEnabled { get; set; } = true;
        public bool BlacklistPrivate { get; set; } = false;
        public bool BlacklistYelling { get; set; } = false;
        public bool WhitelistEnabled { get; set; } = true;
        public bool WhitelistBuddies { get; set; } = false;

        private List<NameFilterItem> _blackItems = new List<NameFilterItem>();
        private List<NameFilterItem> _whiteItems = new List<NameFilterItem>();

        public NameFilterSet(int id) {
            _id = id;
        }

        public void AddBlackList(string pattern, bool permenant) {
            if (IndexOf(pattern, _blackItems) == -1)
                _blackItems.Add(new NameFilterItem(pattern, permenant));
        }

        public void RemoveBlackList(string pattern) {
            int index = IndexOf(pattern, _blackItems);
            if (index != -1)
                _blackItems.RemoveAt(index);
        }

        public bool IsWhitelisted(string pattern) {
            return IndexOf(pattern, _whiteItems) != -1;
        }

        public bool IsBlacklisted(string pattern) {
            return IndexOf(pattern, _blackItems) != -1;
        }

        public bool AcceptMessage(MessageModeType mode, string speaker, string message) {
            if (string.IsNullOrEmpty(speaker))
                return false;

            if (WhitelistEnabled) {
                if (WhitelistBuddies) {
                    // TODO, this should cast on BuddySets that are white.
                }

                if (IndexOf(speaker, _whiteItems) != -1)
                    return true;
            }

            if (BlacklistEnabled) {
                if (BlacklistPrivate && mode == MessageModeType.PrivateFrom)
                    return false;

                if (BlacklistYelling && mode == MessageModeType.Yell)
                    return false;

                if (IndexOf(speaker, _blackItems) != -1)
                    return false;
            }

            return true;
        }

        public int IndexOf(string speaker, List<NameFilterItem> list) {
            speaker = speaker.ToLower();
            int i = 0;
            foreach (var item in list) {
                if (item.Pattern != null && item.Pattern.ToLower() == speaker)
                    return i;
                i++;
            }

            return -1;
        }

        public NameFilterSet Clone() {
            var set = new NameFilterSet(_id);
            set.BlacklistEnabled = BlacklistEnabled;
            set.BlacklistPrivate = BlacklistPrivate;
            set.BlacklistYelling = BlacklistYelling;
            set.WhitelistBuddies = WhitelistBuddies;
            set.WhitelistEnabled = WhitelistEnabled;

            foreach (var item in _blackItems)
                set._blackItems.Add(item.Clone());

            foreach (var item in _whiteItems)
                set._whiteItems.Add(item.Clone());

            return set;
        }
    }
}
