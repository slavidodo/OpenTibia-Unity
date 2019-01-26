using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class MessageFilterSet
    {
        public const int Default = 0;
        public const int NumSets = 1;

        public static MessageFilterSet DefaultFilterSet;

        private int m_ID = 0;
        private MessageMode[] m_MessageModes;

        public bool ShowTimeStamps { get; } = true;
        public bool ShowLevels { get; } = true;

        static MessageFilterSet() {
            DefaultFilterSet = new MessageFilterSet(MessageFilterSet.Default);
        }

        // [TODO] Allowment to have custom filter sets (changing the default colors of anything)
        public MessageFilterSet(int id) {
            m_ID = id;
            m_MessageModes = new MessageMode[(int)MessageModes.BeyondLast];

            for (MessageModes i = MessageModes.None; i < MessageModes.BeyondLast; i++)
                AddMessageMode(new MessageMode(i));
        }

        public MessageMode GetMessageMode(MessageModes mode) {
            if (!MessageMode.s_CheckMode((int)mode))
                throw new System.ArgumentException("MessageModeSet.getMessageMode: Invalid mode: " + mode + ".");

            return m_MessageModes[(int)mode];
        }

        public void AddMessageMode(MessageMode messageMode) {
            m_MessageModes[(int)messageMode.ID] = messageMode;
        }
    }
}
