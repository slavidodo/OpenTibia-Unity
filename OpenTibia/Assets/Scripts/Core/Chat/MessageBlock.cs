using OpenTibiaUnity.Core.WorldMap;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    public class MessageBlock : System.IDisposable
    {
        private bool _timerEventRegistered = false;
        private OnscreenMessageBox _lastOnscreenBox = null;

        private UnityEngine.Vector3Int _position;
        private List<string> _textPieces = new List<string>();
        private int _minTimeForNextOnscreenMessage = 0;
        private int _nextOnscreenMessageIndex = 0;
        private string _speaker;

        public string Speaker {
            get { return _speaker; }
            set { _speaker = value; }
        }

        public MessageBlock(string speaker, UnityEngine.Vector3Int? position) {
            _speaker = speaker ?? throw new System.ArgumentNullException("MessageBlock: speaker is null.");
            _position = position ?? throw new System.Exception("MessageBlock: display position is null.");
        }

        #region _idisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        public void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (_timerEventRegistered) {
                    OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnSecondaryTimer);
                    _timerEventRegistered = false;
                }

                if (disposing && _lastOnscreenBox != null) {
                    _lastOnscreenBox.RemoveMessages();
                    OpenTibiaUnity.GameManager.WorldMapStorage.InvalidateOnscreenMessages();
                }

                _disposedValue = true;
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
            if (_nextOnscreenMessageIndex < _textPieces.Count) {
                if (IsNpcInReach()) {
                    _lastOnscreenBox = OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(_position, 0, _speaker, 0, MessageModeType.NpcFrom,
                        _textPieces[_nextOnscreenMessageIndex]);

                    _minTimeForNextOnscreenMessage = OpenTibiaUnity.TicksMillis + (int)MessageStorage.s_GetTalkDelay(_textPieces[_nextOnscreenMessageIndex]);
                    _nextOnscreenMessageIndex++;
                } else {
                    _nextOnscreenMessageIndex = _textPieces.Count;
                }
            }
        }

        public void AddText(string text) {
            if (text == null)
                throw new System.ArgumentNullException("MessageBlock.AddText: text is null.");

            _textPieces.Add(text);
            MessageModeType mode = _nextOnscreenMessageIndex == 0 ? MessageModeType.NpcFromStartBlock : MessageModeType.NpcFrom;

            OpenTibiaUnity.ChatStorage.AddChannelMessage(ChatStorage.NpcChannelId, 0, _speaker, 0, mode, text);
            if (_nextOnscreenMessageIndex == 0 || _nextOnscreenMessageIndex > 0 && OpenTibiaUnity.TicksMillis > _minTimeForNextOnscreenMessage) {
                ShowNextOnscreenMessage();
            } else if (!_timerEventRegistered) {
                OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnSecondaryTimer);
                _timerEventRegistered = true;
            }
        }

        private void OnSecondaryTimer() {
            if (_nextOnscreenMessageIndex < _textPieces.Count && IsNpcInReach()) {
                if (OpenTibiaUnity.TicksMillis > _minTimeForNextOnscreenMessage) {
                    ShowNextOnscreenMessage();
                }
            } else {
                OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnSecondaryTimer);
                _timerEventRegistered = false;
            }
        }

        private bool IsNpcInReach() {
            var creature = OpenTibiaUnity.CreatureStorage.GetCreatureByName(_speaker);
            if (!creature)
                return false;

            var delta = OpenTibiaUnity.Player.Position - creature.Position;
            return delta.z == 0 && System.Math.Abs(delta.x) <= Constants.MaxNpcDistance && System.Math.Abs(delta.y) <= Constants.MaxNpcDistance;
        }
    }
}
