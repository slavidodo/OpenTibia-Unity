using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    internal class Marks
    {
        
        internal class MarksChangeEvent : UnityEngine.Events.UnityEvent<Marks> { }

        internal const uint MarkNumColors = 216;
        internal const uint MarkAim = MarkNumColors + 1;
        internal const uint MarkAimAttack = MarkNumColors + 2;
        internal const uint MarkAimFollow = MarkNumColors + 3;
        internal const uint MarkAttack = MarkNumColors + 4;
        internal const uint MarkFollow = MarkNumColors + 5;
        internal const uint MarksNumTotal = MarkFollow + 1;
        internal const uint MarkUnmarked = 255;

        internal MarksChangeEvent onMarksChange = new MarksChangeEvent();

        private Dictionary<MarkType, MarkBase> m_CurrentMarks;

        internal Dictionary<MarkType, MarkBase> CurrentMarks { get => m_CurrentMarks; }

        internal uint GetMarkColor(MarkType markTypes) {
            if (m_CurrentMarks != null && m_CurrentMarks.TryGetValue(markTypes, out MarkBase markBase))
                return markBase.MarkColor;

            return MarkUnmarked;
        }

        internal bool AreAnyMarksSet(IEnumerable<MarkType> markTypes) {
            if (m_CurrentMarks == null)
                return false;
            
            foreach (MarkType markType in markTypes) {
                if (IsMarkSet(markType))
                    return true;
            }

            return false;
        }

        internal bool AreAllMarksSet(IEnumerable<MarkType> markTypes) {
            if (m_CurrentMarks == null)
                return false;

            foreach (MarkType markType in markTypes) {
                if (!IsMarkSet(markType))
                    return false;
            }

            return true;
        }

        internal bool IsMarkSet(MarkType markType) {
            if (m_CurrentMarks == null)
                return false;

            MarkBase markBase;
            if (m_CurrentMarks.TryGetValue(markType, out markBase)) {
                return markBase.IsSet;
            }

            return false;
        }

        internal void Clear() {
            if (m_CurrentMarks != null && m_CurrentMarks.Count > 0) {
                m_CurrentMarks.Clear();

                onMarksChange.Invoke(this);
            }
        }

        internal void SetMark(MarkType markType, uint color) {
            if (m_CurrentMarks == null) {
                if (color == MarkUnmarked)
                    return;
                else
                    m_CurrentMarks = new Dictionary<MarkType, MarkBase>();
            }

            MarkBase _;
            if (!m_CurrentMarks.TryGetValue(markType, out _)) {
                if (markType == MarkType.OneSecondTemp)
                    m_CurrentMarks.Add(MarkType.OneSecondTemp, new MarkTimeOut(1000));
                else
                    m_CurrentMarks.Add(markType, new MarkBase());
            }

            if (GetMarkColor(markType) != color) {
                m_CurrentMarks[markType].Set(color);

                onMarksChange.Invoke(this);
            }
        }
        
        public static bool operator !(Marks instance) {
            return instance == null;
        }

        public static bool operator true(Marks instance) {
            return !(!instance);
        }

        public static bool operator false(Marks instance) {
            return !(instance);
        }
    }

    internal class MarkBase
    {
        protected uint m_MarkColor = 0;
        internal uint MarkColor { get { return m_MarkColor; } }

        internal virtual void Set(uint color) {
            m_MarkColor = color;
        }

        internal virtual bool IsSet {
            get {
                return m_MarkColor != Marks.MarkUnmarked;
            }
        }
    }

    internal class MarkTimeOut : MarkBase
    {
        System.Timers.Timer m_Timer;
        int m_TimeoutMilliseconds;
        int m_SetTime;

        internal MarkTimeOut(int millis) {
            m_TimeoutMilliseconds = millis;
        }

        internal override void Set(uint color) {
            m_MarkColor = 0;
            m_SetTime = OpenTibiaUnity.TicksMillis;
        }

        internal override bool IsSet {
            get {
                return base.IsSet && m_SetTime + m_TimeoutMilliseconds > OpenTibiaUnity.TicksMillis;
            }
        }
    }
}
