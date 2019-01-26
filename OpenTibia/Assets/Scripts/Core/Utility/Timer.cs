namespace OpenTibiaUnity.Core.Utility
{
    public class Timer {
        public class TimerEvent : UnityEngine.Events.UnityEvent<object, System.Timers.ElapsedEventArgs> {};

        private System.Timers.Timer m_SystemTimer;
        private TimerEvent m_TimerEvent = new TimerEvent();

        public double Interval {
            get { return m_SystemTimer.Interval; }
            set { m_SystemTimer.Interval = value; }
        }

        public Timer(double interval, bool autoStart = false) {
            m_SystemTimer = new System.Timers.Timer(interval);
            m_SystemTimer.Elapsed += OnSystemTimerFinishes;

            if (autoStart) {
                m_SystemTimer.Start();
            }
        }

        public void Start() => m_SystemTimer?.Start();
        public void Stop() => m_SystemTimer?.Stop();
        public void Dispose() => m_SystemTimer?.Dispose();

        public void AddListener(UnityEngine.Events.UnityAction<object, System.Timers.ElapsedEventArgs> action) {
            m_TimerEvent.AddListener(action);
        }

        public void RemoveListener(UnityEngine.Events.UnityAction<object, System.Timers.ElapsedEventArgs> action) {
            m_TimerEvent.RemoveListener(action);
        }

        public void RemoveAllListeners() {
            m_TimerEvent.RemoveAllListeners();
        }

        public void OnSystemTimerFinishes(object state, System.Timers.ElapsedEventArgs e) {     
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                m_TimerEvent.Invoke(state, e);
            });
        }
    }
}
