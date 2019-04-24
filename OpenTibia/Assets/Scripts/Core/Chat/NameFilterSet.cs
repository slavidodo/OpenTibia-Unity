using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class NameFilterSet
    {
        public static int DefaultSet = 0;

        private int m_ID;

        public bool BlacklistEnabled { get; set; } = true;
        public bool BlacklistPrivate { get; set; } = false;
        public bool BlacklistYelling { get; set; } = false;
        public bool WhitelistEnabled { get; set; } = true;
        public bool WhitelistBuddies { get; set; } = false;

        private List<NameFilterItem> m_BlackItems = new List<NameFilterItem>();
        private List<NameFilterItem> m_WhiteItems = new List<NameFilterItem>();

        public NameFilterSet(int id) {
            m_ID = id;
        }

        public void AddBlackList(string pattern, bool permenant) {
            if (IndexOf(pattern, m_BlackItems) == -1)
                m_BlackItems.Add(new NameFilterItem(pattern, permenant));
        }

        public void RemoveBlackList(string pattern) {
            int index = IndexOf(pattern, m_BlackItems);
            if (index != -1)
                m_BlackItems.RemoveAt(index);
        }

        public bool IsWhitelisted(string pattern) {
            return IndexOf(pattern, m_WhiteItems) != -1;
        }

        public bool IsBlacklisted(string pattern) {
            return IndexOf(pattern, m_BlackItems) != -1;
        }

        public bool AcceptMessage(MessageModes mode, string speaker, string message) {
            if (WhitelistEnabled) {
                if (WhitelistBuddies) {
                    // TODO, this should cast on BuddySets that are white.
                }

                if (IndexOf(speaker, m_WhiteItems) != -1)
                    return true;
            }

            if (BlacklistEnabled) {
                if (BlacklistPrivate && mode == MessageModes.PrivateFrom)
                    return false;

                if (BlacklistYelling && mode == MessageModes.Yell)
                    return false;

                if (IndexOf(speaker, m_BlackItems) != -1)
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
            var set = new NameFilterSet(m_ID);
            set.BlacklistEnabled = BlacklistEnabled;
            set.BlacklistPrivate = BlacklistPrivate;
            set.BlacklistYelling = BlacklistYelling;
            set.WhitelistBuddies = WhitelistBuddies;
            set.WhitelistEnabled = WhitelistEnabled;

            foreach (var item in m_BlackItems)
                set.m_BlackItems.Add(item.Clone());

            foreach (var item in m_WhiteItems)
                set.m_WhiteItems.Add(item.Clone());

            return set;
        }
    }
}
