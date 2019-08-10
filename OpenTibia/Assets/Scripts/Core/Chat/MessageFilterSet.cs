using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    internal class MessageFilterSet
    {
        internal const int DefaultSet = 0;
        internal const int NumSets = 1;
        
        private int m_ID = 0;
        private MessageMode[] m_MessageModes;

        internal bool ShowTimeStamps { get; } = true;
        internal bool ShowLevels { get; } = true;

        // [TODO] Allowment to have custom filter sets (changing the default colors of anything)
        internal MessageFilterSet(int id) {
            m_ID = id;
            m_MessageModes = new MessageMode[(int)MessageModeType.Invalid];

            for (MessageModeType i = MessageModeType.None; i < MessageModeType.BeyondLast; i++)
                AddMessageMode(new MessageMode(i));
            
            AddMessageMode(new MessageMode(MessageModeType.MonsterYell));
            AddMessageMode(new MessageMode(MessageModeType.MonsterSay));
            AddMessageMode(new MessageMode(MessageModeType.Red));
            AddMessageMode(new MessageMode(MessageModeType.Blue));
            AddMessageMode(new MessageMode(MessageModeType.RVRChannel));
            AddMessageMode(new MessageMode(MessageModeType.RVRAnswer));
            AddMessageMode(new MessageMode(MessageModeType.RVRContinue));
            AddMessageMode(new MessageMode(MessageModeType.GameHighlight));
            AddMessageMode(new MessageMode(MessageModeType.NpcFromStartBlock));
        }

        internal MessageMode GetMessageMode(MessageModeType mode) {
            if (!MessageMode.s_CheckMode((int)mode))
                throw new System.ArgumentException("MessageModeSet.getMessageMode: Invalid mode: " + (int)mode + ".");

            return m_MessageModes[(int)mode];
        }

        internal void AddMessageMode(MessageMode messageMode) {
            m_MessageModes[(int)messageMode.ID] = messageMode;
        }
    }
}
