using OpenTibiaUnity.Core.WorldMap;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class MessageBlock : System.IDisposable
    {
        private bool m_TimerEventRegistered = false;
        private OnscreenMessageBox m_LastOnscreenBox = null;

        private UnityEngine.Vector3Int m_Position;
        private List<string> m_TextPieces = new List<string>();
        private int m_MinTimeForNextOnscreenMessage = 0;
        private int m_NextOnscreenMessageIndex = 0;
        public string m_Speaker;

        public string Speaker {
            get { return m_Speaker; }
            set { m_Speaker = value; }
        }

        public MessageBlock(string speaker, UnityEngine.Vector3Int? position) {
            m_Speaker = speaker ?? throw new System.ArgumentNullException("MessageBlock: speaker is null.");
            m_Position = position ?? throw new System.Exception("MessageBlock: display position is null.");
        }

        #region IDisposable Support
        private bool m_DisposedValue = false; // To detect redundant calls

        public void Dispose(bool disposing) {
            if (!m_DisposedValue) {
                if (m_TimerEventRegistered) {
                    OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnSecondaryTimer);
                    m_TimerEventRegistered = false;
                }

                if (disposing && m_LastOnscreenBox != null) {
                    m_LastOnscreenBox.RemoveMessages();
                    OpenTibiaUnity.GameManager.WorldMapStorage.InvalidateOnscreenMessages();
                }

                m_DisposedValue = true;
            }
        }
        
        ~MessageBlock() {
          Dispose(false);
        }
        
        public void Dispose() {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
        #endregion

        protected void ShowNextOnscreenMessage() {
            if (m_NextOnscreenMessageIndex < m_TextPieces.Count) {
                if (IsNpcInReach()) {
                    m_LastOnscreenBox = OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(m_Position, 0, m_Speaker, 0, MessageModeType.NpcFrom,
                        m_TextPieces[m_NextOnscreenMessageIndex]);

                    m_MinTimeForNextOnscreenMessage = OpenTibiaUnity.TicksMillis + (int)MessageStorage.s_GetTalkDelay(m_TextPieces[m_NextOnscreenMessageIndex]);
                    m_NextOnscreenMessageIndex++;
                } else {
                    m_NextOnscreenMessageIndex = m_TextPieces.Count;
                }
            }
        }

        public void AddText(string text) {
            if (text == null)
                throw new System.ArgumentNullException("MessageBlock.AddText: text is null.");

            m_TextPieces.Add(text);
            MessageModeType mode = m_NextOnscreenMessageIndex == 0 ? MessageModeType.NpcFromStartBlock : MessageModeType.NpcFrom;

            OpenTibiaUnity.ChatStorage.AddChannelMessage(ChatStorage.NpcChannelID, 0, m_Speaker, 0, mode, text);
            if (m_NextOnscreenMessageIndex == 0 || m_NextOnscreenMessageIndex > 0 && OpenTibiaUnity.TicksMillis > m_MinTimeForNextOnscreenMessage) {
                ShowNextOnscreenMessage();
            } else if (!m_TimerEventRegistered) {
                OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnSecondaryTimer);
                m_TimerEventRegistered = true;
            }
        }

        private void OnSecondaryTimer() {
            if (m_NextOnscreenMessageIndex < m_TextPieces.Count && IsNpcInReach()) {
                if (OpenTibiaUnity.TicksMillis > m_MinTimeForNextOnscreenMessage) {
                    ShowNextOnscreenMessage();
                }
            } else {
                OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnSecondaryTimer);
                m_TimerEventRegistered = false;
            }
        }

        private bool IsNpcInReach() {
            var creature = OpenTibiaUnity.CreatureStorage.GetCreatureByName(m_Speaker);
            if (!creature)
                return false;

            var delta = OpenTibiaUnity.Player.Position - creature.Position;
            return delta.z == 0 && System.Math.Abs(delta.x) <= Constants.MaxNpcDistance && System.Math.Abs(delta.y) <= Constants.MaxNpcDistance;
        }
    }
}
