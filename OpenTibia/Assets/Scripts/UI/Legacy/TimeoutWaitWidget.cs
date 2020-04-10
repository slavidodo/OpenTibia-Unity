using System;
using UnityEngine;

namespace OpenTibiaUnity.UI.Legacy
{
    public class TimeoutWaitWidget : MessageWidgetBase
    {
        // fields
        private int _showTimestamp = -1;
        private int _timeOut = -1;
        private string _customMessage = string.Empty;

        public int TimeOut {
            get => _timeOut;
            set {
                if (_timeOut != value) {
                    _timeOut = value;
                    UpdateMessage();
                }
            }
        }

        public int ElapsedTime {
            get {
                var millis = OpenTibiaUnity.GameManager.TicksMillis;
                if (_showTimestamp > -1)
                    return millis - _showTimestamp;
                return millis;
            }
        }

        public int RemainingTime {
            get {
                if (_showTimestamp > -1)
                    return  _showTimestamp + _timeOut - OpenTibiaUnity.GameManager.TicksMillis;
                return _timeOut;
            }
        }

        public override string message {
            get => _customMessage;
            set => _customMessage = value;
        }

        protected override void Awake() {
            base.Awake();

            KeyMask = PopUpKeyMask.Escape;
        }

        private void OnTimer() {
            int remaining = RemainingTime;
            if (remaining <= 0) {
                OpenTibiaUnity.GameManager.onSecondaryTimeCheck.RemoveListener(OnTimer);
                OnTimeoutOccurred();
            } else {
                UpdateMessage();
            }
        }

        public override void Show() {
            base.Show();

            if (_showTimestamp < 0) {
                _showTimestamp = OpenTibiaUnity.TicksMillis;
                OpenTibiaUnity.GameManager.onSecondaryTimeCheck.AddListener(OnTimer);
            }
        }

        protected virtual void UpdateMessage() {
        }

        protected virtual void OnTimeoutOccurred() {
            onClose.Invoke(this);
        }

        protected static string GetTimeString(int time) {
            string result;
            if (time > 60000) {
                int minutes = (int)Mathf.Ceil(time / 60000f);
                if (minutes == 1)
                    result = string.Format(TextResources.LABEL_MINUTE, minutes);
                else
                    result = string.Format(TextResources.LABEL_MINUTES, minutes);
            } else {
                int seconds = (int)Mathf.Ceil(time / 60000f);
                if (seconds == 1)
                    result = string.Format(TextResources.LABEL_SECOND, seconds);
                else
                    result = string.Format(TextResources.LABEL_SECONDS, seconds);
            }
            return result; 
        }
    }
}
