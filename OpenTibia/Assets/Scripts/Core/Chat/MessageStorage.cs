namespace OpenTibiaUnity.Core.Chat
{
    public class MessageStorage
    {
        public const uint MessageBaseDelay = 1000;

        private MessageBlock _currentBlock = null;

        public MessageBlock GetMessageBlock(string speaker) {
            if (speaker == null)
                throw new System.ArgumentNullException("MessageStorage.GetMessageBlock: speaker is null");

            if (_currentBlock != null && _currentBlock.Speaker == speaker)
                return _currentBlock;

            return null;
        }

        public void StartMessageBlock(string speaker, UnityEngine.Vector3Int? position, string text) {
            if (speaker == null)
                return;

            if (GetMessageBlock(speaker) != null)
                _currentBlock.Dispose(true);

            if (_currentBlock != null)
                _currentBlock.Dispose(false);

            _currentBlock = new MessageBlock(speaker, position);
            _currentBlock.AddText(text);
        }

        public void AddTextToBlock(string speaker, string text) {
            MessageBlock messageBlock = GetMessageBlock(speaker);
            if (messageBlock != null)
                messageBlock.AddText(text);
        }

        public static uint s_GetTalkDelay(string text) {
            return MessageBaseDelay + (uint)(4000 * text.Length / 256f + 3000);
        }
    }
}
