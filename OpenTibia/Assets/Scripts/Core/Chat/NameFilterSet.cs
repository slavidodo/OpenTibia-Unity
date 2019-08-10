using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    internal class NameFilterSet
    {
        internal static int DefaultSet = 0;

        private int m_ID;

        internal bool BlacklistEnabled { get; set; } = true;
        internal bool BlacklistPrivate { get; set; } = false;
        internal bool BlacklistYelling { get; set; } = false;
        internal bool WhitelistEnabled { get; set; } = true;
        internal bool WhitelistBuddies { get; set; } = false;

        private List<NameFilterItem> m_BlackItems = new List<NameFilterItem>();
        private List<NameFilterItem> m_WhiteItems = new List<NameFilterItem>();

        internal NameFilterSet(int id) {
            m_ID = id;
        }

        internal void AddBlackList(string pattern, bool permenant) {
            if (IndexOf(pattern, m_BlackItems) == -1)
                m_BlackItems.Add(new NameFilterItem(pattern, permenant));
        }

        internal void RemoveBlackList(string pattern) {
            int index = IndexOf(pattern, m_BlackItems);
            if (index != -1)
                m_BlackItems.RemoveAt(index);
        }

        internal bool IsWhitelisted(string pattern) {
            return IndexOf(pattern, m_WhiteItems) != -1;
        }

        internal bool IsBlacklisted(string pattern) {
            return IndexOf(pattern, m_BlackItems) != -1;
        }

        internal bool AcceptMessage(MessageModeType mode, string speaker, string message) {
            if (string.IsNullOrEmpty(speaker))
                return false;

            if (WhitelistEnabled) {
                if (WhitelistBuddies) {
                    // TODO, this should cast on BuddySets that are white.
                }

                if (IndexOf(speaker, m_WhiteItems) != -1)
                    return true;
            }

            if (BlacklistEnabled) {
                if (BlacklistPrivate && mode == MessageModeType.PrivateFrom)
                    return false;

                if (BlacklistYelling && mode == MessageModeType.Yell)
                    return false;

                if (IndexOf(speaker, m_BlackItems) != -1)
                    return false;
            }

            return true;
        }

        internal int IndexOf(string speaker, List<NameFilterItem> list) {
            speaker = speaker.ToLower();
            int i = 0;
            foreach (var item in list) {
                if (item.Pattern != null && item.Pattern.ToLower() == speaker)
                    return i;
                i++;
            }

            return -1;
        }

        internal NameFilterSet Clone() {
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
