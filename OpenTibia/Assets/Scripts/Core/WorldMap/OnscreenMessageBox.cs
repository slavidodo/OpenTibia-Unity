using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class OnscreenMessageBox
    {
        public Utils.RingBuffer<OnscreenMessage> _messages;
        private Vector3Int? _position;
        private int _speakerLevel;
        private int m_VisibleMessages = 0;
        private TMPro.TextMeshProUGUI _textMesh;

        private float _width = 0;
        private float _height = 0;

        public Vector3Int? Position {
            get { return _position; }
        }

        public MessageModeType Mode { get; }
        public string Speaker { get; }
        public bool Visible {
            get { return _textMesh != null ? _textMesh.gameObject.activeSelf : false; }
            set { if (_textMesh) _textMesh.gameObject.SetActive(value); }
        }
        public int VisibleMessages {
            get { return !!Visible ? m_VisibleMessages : 0; }
        }
        public bool Empty {
            get { return _messages.Length - GetFirstNonHeaderIndex() <= 0; }
        }

        public int MinExpirationTime {
            get {
                var min = int.MaxValue;
                var riskIndex = GetFirstNonHeaderIndex();
                for (int i = riskIndex; i <  _messages.Length; i++) {
                    var message = _messages.GetItemAt(i);
                    if (message.VisibleSince < int.MaxValue) {
                        min = System.Math.Min(message.VisibleSince + message.TTL, min);
                    }
                }

                return min;
            }
        }

        public OnscreenMessageBox(Vector3Int? position, string speaker, int speakerLevel,MessageModeType mode, int messagesSize, TMPro.TextMeshProUGUI textMesh = null) {
            _position = position;
            Speaker = speaker;
            _speakerLevel = speakerLevel;
            Mode = mode;
            _messages = new Utils.RingBuffer<OnscreenMessage>(messagesSize);
            _textMesh = textMesh;
        }

        public void DestroyTextMesh() {
            if (_textMesh != null) {
                Object.Destroy(_textMesh.gameObject);
                _textMesh = null;
            }
        }

        public void RemoveMessages() {
            _messages.RemoveAll();
        }

        public int ExpireMessages(int ticks) {
            int totalExprired = 0;
            if (Visible) {
                int index = GetFirstNonHeaderIndex();
                while (_messages.Length > index) {
                    var message = _messages.GetItemAt(index);
                    if (message.VisibleSince < ticks && message.VisibleSince + message.TTL < ticks) {
                        _messages.RemoveItemAt(index);
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
            _height = 0;
            _width = 0;

            if (!Visible)
                return;
            
            switch (Mode) {
                case MessageModeType.Say:
                case MessageModeType.Whisper:
                case MessageModeType.Yell:
                case MessageModeType.Spell:
                case MessageModeType.NpcFrom:
                case MessageModeType.BarkLoud:
                case MessageModeType.BarkLow:
                    int i = 0;
                    while (m_VisibleMessages < _messages.Length) {
                        var onscreenMessage = _messages.GetItemAt(m_VisibleMessages);
                        var size = _textMesh.GetPreferredValues(onscreenMessage.RichText);
                        if (_height + size.y <= Constants.OnscreenMessageHeight) {
                            m_VisibleMessages++;
                            _width = Mathf.Max(_width, size.x);
                            _height += size.y;
                            onscreenMessage.VisibleSince = Mathf.Min(OpenTibiaUnity.TicksMillis, onscreenMessage.VisibleSince);

                            _textMesh.text += (i++ == 0 ? "" : "\n") + onscreenMessage.RichText;
                            continue;
                        }
                        break;
                    }

                    break;
                default:
                    if (_messages.Length > 0) {
                        var onscreenMessage = _messages.GetItemAt(0);
                        _textMesh.text = onscreenMessage.RichText;
                        var size = _textMesh.GetPreferredValues();
                        m_VisibleMessages = 1;
                        _width = size.x;
                        _height = size.y;
                        onscreenMessage.VisibleSince = Mathf.Min(OpenTibiaUnity.TicksMillis, onscreenMessage.VisibleSince);
                    }
                    break;
            }

            _textMesh.ForceMeshUpdate();
        }

        public void AppendMessage(OnscreenMessage message) {
            if (_messages.Length >= _messages.Size)
                _messages.RemoveItemAt(GetFirstNonHeaderIndex());
            _messages.AddItem(message);
        }

        public int ExpireOldestMessage() {
            if (Visible) {
                var riskIndex = GetFirstNonHeaderIndex();
                if (_messages.Length > riskIndex) {
                    _messages.RemoveItemAt(riskIndex);
                    return 1;
                }
            }

            return 0;
        }

        public int GetFirstNonHeaderIndex() {
            int i = 0;
            while (_messages.Length > i) {
                var message = _messages.GetItemAt(i);
                if (message.TTL < int.MaxValue)
                    break;
                i++;
            }

            return i;
        }

        public void ResetTextMesh() {
            _textMesh.text = string.Empty;
        }

        public void UpdateTextMeshPosition(float x, float y) {
            var rectTransform = _textMesh.rectTransform;

            float width = Mathf.Min(_textMesh.preferredWidth, Constants.OnscreenMessageWidth);
            float height = _textMesh.preferredHeight;
            
            var parentRT = rectTransform.parent as RectTransform;
            
            x = Mathf.Clamp(x, width / 2, parentRT.rect.width - width / 2);
            y = Mathf.Clamp(y, -parentRT.rect.height + height / 2, -height / 2);

            if (rectTransform.anchoredPosition.x == x && rectTransform.anchoredPosition.y == y)
                return;

            rectTransform.anchoredPosition = new Vector3(x, y, 0);
        }
    }
}