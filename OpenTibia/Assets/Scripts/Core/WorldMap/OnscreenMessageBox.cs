using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap
{
    public enum OnscreenMessageFixing
    {
        None = 0,
        X = 1,
        Y = 2,
        Both = X | Y,
    }

    public class OnscreenMessageBox
    {
        private Utils.RingBuffer<OnscreenMessage> _messages;
        private Vector3Int? _position;
        private int _speakerLevel;
        private int _visibleMessages = 0;

        private float _width = 0;
        private float _height = 0;
        private bool _visible = true;
        private OnscreenMessageFixing _fixing = 0;

        public Vector3Int? Position { get => _position; }
        public OnscreenMessageFixing Fixing { get => _fixing; set => _fixing = value; }

        public MessageModeType Mode { get; }
        public string Speaker { get; }
        public bool Visible { get => _visible; set => _visible = value; }
        public int VisibleMessages { get => !!Visible ? _visibleMessages : 0; }
        public bool Empty { get => _messages.Length - GetFirstNonHeaderIndex() <= 0; }
        public float Width { get => _width; }
        public float Height { get => _height; }

        // assigned after layouting
        public float X { get; set; }
        public float Y { get; set; }

        public int MinExpirationTime {
            get {
                var min = int.MaxValue;
                var riskIndex = GetFirstNonHeaderIndex();
                for (int i = riskIndex; i <  _messages.Length; i++) {
                    var message = _messages.GetItemAt(i);
                    if (message.VisibleSince < int.MaxValue)
                        min = System.Math.Min(message.VisibleSince + message.TTL, min);
                }

                return min;
            }
        }

        public OnscreenMessageBox(Vector3Int? position, string speaker, int speakerLevel, MessageModeType mode, int messagesSize) {
            _position = position;
            Speaker = speaker;
            _speakerLevel = speakerLevel;
            Mode = mode;
            _messages = new Utils.RingBuffer<OnscreenMessage>(messagesSize);
        }
        
        public void RemoveMessages() {
            _messages.RemoveAll();
        }

        public OnscreenMessage GetMessage(int index) {
            return _messages.GetItemAt(index);
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
            _visibleMessages = 0;
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
                    while (_visibleMessages < _messages.Length) {
                        var onscreenMessage = _messages.GetItemAt(_visibleMessages);
                        if (_height + onscreenMessage.Height <= Constants.OnscreenMessageHeight) {
                            _visibleMessages++;
                            _width = Mathf.Max(_width, onscreenMessage.Width);
                            _height += onscreenMessage.Height;
                            onscreenMessage.VisibleSince = Mathf.Min(OpenTibiaUnity.TicksMillis, onscreenMessage.VisibleSince);
                            continue;
                        }
                        break;
                    }
                    break;
                default:
                    if (_messages.Length > 0) {
                        var onscreenMessage = _messages.GetItemAt(0);
                        _visibleMessages = 1;
                        _width = onscreenMessage.Width;
                        _height = onscreenMessage.Height;
                        onscreenMessage.VisibleSince = Mathf.Min(OpenTibiaUnity.TicksMillis, onscreenMessage.VisibleSince);
                    }
                    break;
            }
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
    }
}