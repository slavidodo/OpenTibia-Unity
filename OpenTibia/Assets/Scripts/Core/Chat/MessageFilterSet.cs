namespace OpenTibiaUnity.Core.Chat
{
    public class MessageFilterSet
    {
        public const int DefaultSet = 0;
        public const int NumSets = 1;
        
        private int _id = 0;
        private MessageMode[] _messageModes;

        public bool ShowTimeStamps { get; } = true;
        public bool ShowLevels { get; } = true;

        // [TODO] Allowment to have custom filter sets (changing the default colors of anything)
        public MessageFilterSet(int id) {
            _id = id;
            _messageModes = new MessageMode[(int)MessageModeType.LastMessage];

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

        public MessageMode GetMessageMode(MessageModeType mode) {
            if (!MessageMode.s_CheckMode((int)mode))
                throw new System.ArgumentException("MessageModeSet.getMessageMode: Invalid mode: " + (int)mode + ".");

            return _messageModes[(int)mode];
        }

        public void AddMessageMode(MessageMode messageMode) {
            _messageModes[(int)messageMode.Id] = messageMode;
        }
    }
}
