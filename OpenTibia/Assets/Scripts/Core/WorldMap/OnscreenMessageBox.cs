using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class OnscreenMessageBox
    {
        public Utility.RingBuffer<OnscreenMessage> m_Messages;
        private Vector3Int? m_Position;
        private int m_SpeakerLevel;
        private int m_VisibleMessages = 0;
        private TMPro.TextMeshProUGUI m_TextMesh;

        private float m_Width = 0;
        private float m_Height = 0;

        public Vector3Int? Position {
            get { return m_Position; }
        }

        public MessageModes Mode { get; }
        public string Speaker { get; }
        public bool Visible {
            get { return m_TextMesh != null ? m_TextMesh.gameObject.activeSelf : false; }
            set { if (m_TextMesh) m_TextMesh.gameObject.SetActive(value); }
        }
        public int VisibleMessages {
            get { return !!Visible ? m_VisibleMessages : 0; }
        }
        public bool Empty {
            get { return m_Messages.Length - GetFirstNonHeaderIndex() <= 0; }
        }

        public int MinExpirationTime {
            get {
                var min = int.MaxValue;
                var riskIndex = GetFirstNonHeaderIndex();
                for (int i = riskIndex; i <  m_Messages.Length; i++) {
                    var message = m_Messages.GetItemAt(i);
                    if (message.VisibleSince < int.MaxValue) {
                        min = System.Math.Min(message.VisibleSince + message.TTL, min);
                    }
                }

                return min;
            }
        }

        public OnscreenMessageBox(Vector3Int? position, string speaker, int speakerLevel,MessageModes mode, int messagesSize, TMPro.TextMeshProUGUI textMesh = null) {
            m_Position = position;
            Speaker = speaker;
            m_SpeakerLevel = speakerLevel;
            Mode = mode;
            m_Messages = new Utility.RingBuffer<OnscreenMessage>(messagesSize);
            m_TextMesh = textMesh;
        }

        public void DestroyTextMesh() {
            if (m_TextMesh != null) {
                Object.Destroy(m_TextMesh.gameObject);
                m_TextMesh = null;
            }
        }

        public void RemoveMessages() {
            m_Messages.RemoveAll();
        }

        public int ExpireMessages(int ticks) {
            int totalExprired = 0;
            if (Visible) {
                var index = GetFirstNonHeaderIndex();
                while (m_Messages.Length > index) {
                    var message = m_Messages.GetItemAt(index);
                    if (message.VisibleSince < ticks && message.VisibleSince + message.TTL < ticks) {
                        m_Messages.RemoveItemAt(index);
                        totalExprired++;
                        continue;
                    }
                    break;
                }
            }

            return totalExprired;
        }

        public void ArrangeMessages() {
            m_VisibleMessages = 0;
            m_Height = 0;
            m_Width = 0;

            if (!Visible)
                return;
            
            switch (Mode) {
                case MessageModes.Say:
                case MessageModes.Whisper:
                case MessageModes.Yell:
                case MessageModes.Spell:
                case MessageModes.NpcFrom:
                case MessageModes.BarkLoud:
                case MessageModes.BarkLow:
                    int i = 0;
                    while (m_VisibleMessages < m_Messages.Length) {
                        var onscreenMessage = m_Messages.GetItemAt(m_VisibleMessages);
                        var size = m_TextMesh.GetPreferredValues(onscreenMessage.RichText);
                        if (m_Height + size.y <= Constants.OnscreenMessageHeight) {
                            m_VisibleMessages++;
                            m_Width = Mathf.Max(m_Width, size.x);
                            m_Height += size.y;
                            onscreenMessage.VisibleSince = Mathf.Min(OpenTibiaUnity.TicksMillis, onscreenMessage.VisibleSince);

                            m_TextMesh.text += (i++ == 0 ? "" : "\n") + onscreenMessage.RichText;
                            continue;
                        }
                        break;
                    }

                    break;
                default:
                    if (m_Messages.Length > 0) {
                        var onscreenMessage = m_Messages.GetItemAt(0);
                        m_TextMesh.text = onscreenMessage.RichText;
                        var size = m_TextMesh.GetPreferredValues();
                        m_VisibleMessages = 1;
                        m_Width = size.x;
                        m_Height = size.y;
                        onscreenMessage.VisibleSince = Mathf.Min(OpenTibiaUnity.TicksMillis, onscreenMessage.VisibleSince);
                    }
                    break;
            }

            m_TextMesh.ForceMeshUpdate();
        }

        public void AppendMessage(OnscreenMessage message) {
            if (m_Messages.Length >= m_Messages.Size)
                m_Messages.RemoveItemAt(GetFirstNonHeaderIndex());
            m_Messages.AddItem(message);
        }

        public int ExpireOldestMessage() {
            if (Visible) {
                var riskIndex = GetFirstNonHeaderIndex();
                if (m_Messages.Length > riskIndex) {
                    m_Messages.RemoveItemAt(riskIndex);
                    return 1;
                }
            }

            return 0;
        }

        public int GetFirstNonHeaderIndex() {
            int i = 0;
            while (m_Messages.Length > i) {
                var message = m_Messages.GetItemAt(i);
                if (message.TTL < int.MaxValue)
                    break;
                i++;
            }

            return i;
        }

        public void ResetTextMesh() {
            m_TextMesh.text = string.Empty;
        }

        public void UpdateTextMeshPosition(float x, float y) {
            var rectTransform = m_TextMesh.transform as RectTransform;

            float width = Mathf.Min(m_TextMesh.preferredWidth, Constants.OnscreenMessageWidth);
            float height = m_TextMesh.preferredHeight;

            var parentRT = rectTransform.transform.parent as RectTransform;
            
            x = Mathf.Clamp(x, width / 2, parentRT.rect.width - width / 2);
            y = Mathf.Clamp(y, -parentRT.rect.height + height / 2, -height / 2);

            if (rectTransform.anchoredPosition.x == x && rectTransform.anchoredPosition.y == y)
                return;

            rectTransform.anchoredPosition = new Vector3(x, y, 0);
        }
    }
}