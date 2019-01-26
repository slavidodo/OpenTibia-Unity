namespace OpenTibiaUnity.Core.Chat
{
    public class MessageStorage
    {
        public const uint MessageBaseDelay = 1000;

        private MessageBlock m_CurrentBlock = null;

        public MessageBlock GetMessageBlock(string speaker) {
            if (speaker == null)
                throw new System.ArgumentNullException("MessageStorage.GetMessageBlock: speaker is null");

            if (m_CurrentBlock != null && m_CurrentBlock.Speaker == speaker)
                return m_CurrentBlock;

            return null;
        }

        public void StartMessageBlock(string speaker, UnityEngine.Vector3Int? position, string text) {
            if (speaker == null)
                return;

            if (GetMessageBlock(speaker) != null)
                m_CurrentBlock.Dispose(true);

            if (m_CurrentBlock != null)
                m_CurrentBlock.Dispose(false);

            m_CurrentBlock = new MessageBlock(speaker, position);
            m_CurrentBlock.AddText(text);
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
