namespace OpenTibiaUnity.Core.Utils
{
    public class Timer {
        public class TimerEvent : UnityEngine.Events.UnityEvent<object, System.Timers.ElapsedEventArgs> {};

        private System.Timers.Timer _systemTimer;
        private TimerEvent _timerEvent = new TimerEvent();

        public double Interval {
            get { return _systemTimer.Interval; }
            set { _systemTimer.Interval = value; }
        }

        public Timer(double interval, bool autoStart = false) {
            _systemTimer = new System.Timers.Timer(interval);
            _systemTimer.Elapsed += OnSystemTimerFinishes;

            if (autoStart) {
                _systemTimer.Start();
            }
        }

        public void Start() => _systemTimer?.Start();
        public void Stop() => _systemTimer?.Stop();
        public void Dispose() => _systemTimer?.Dispose();

        public void AddListener(UnityEngine.Events.UnityAction<object, System.Timers.ElapsedEventArgs> action) {
            _timerEvent.AddListener(action);
        }

        public void RemoveListener(UnityEngine.Events.UnityAction<object, System.Timers.ElapsedEventArgs> action) {
            _timerEvent.RemoveListener(action);
        }

        public void RemoveAllListeners() {
            _timerEvent.RemoveAllListeners();
        }

        public void OnSystemTimerFinishes(object state, System.Timers.ElapsedEventArgs e) {     
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                _timerEvent.Invoke(state, e);
            });
        }
    }
}
