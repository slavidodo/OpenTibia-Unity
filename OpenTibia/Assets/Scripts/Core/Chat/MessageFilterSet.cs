using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class MessageFilterSet
    {
        public const int DefaultSet = 0;
        public const int NumSets = 1;
        
        private int m_ID = 0;
        private MessageMode[] m_MessageModes;

        public bool ShowTimeStamps { get; } = true;
        public bool ShowLevels { get; } = true;

        // [TODO] Allowment to have custom filter sets (changing the default colors of anything)
        public MessageFilterSet(int id) {
            m_ID = id;
            m_MessageModes = new MessageMode[(int)MessageModes.Invalid];

            for (MessageModes i = MessageModes.None; i < MessageModes.BeyondLast; i++)
                AddMessageMode(new MessageMode(i));
            
            AddMessageMode(new MessageMode(MessageModes.MonsterYell));
            AddMessageMode(new MessageMode(MessageModes.MonsterSay));
            AddMessageMode(new MessageMode(MessageModes.Red));
            AddMessageMode(new MessageMode(MessageModes.Blue));
            AddMessageMode(new MessageMode(MessageModes.RVRChannel));
            AddMessageMode(new MessageMode(MessageModes.RVRAnswer));
            AddMessageMode(new MessageMode(MessageModes.RVRContinue));
            AddMessageMode(new MessageMode(MessageModes.GameHighlight));
            AddMessageMode(new MessageMode(MessageModes.NpcFromStartBlock));
        }

        public MessageMode GetMessageMode(MessageModes mode) {
            if (!MessageMode.s_CheckMode((int)mode))
                throw new System.ArgumentException("MessageModeSet.getMessageMode: Invalid mode: " + (int)mode + ".");

            return m_MessageModes[(int)mode];
        }

        public void AddMessageMode(MessageMode messageMode) {
            m_MessageModes[(int)messageMode.ID] = messageMode;
        }
    }
}
