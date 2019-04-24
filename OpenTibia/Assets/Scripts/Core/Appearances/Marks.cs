using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    public class Marks
    {
        
        public class MarksChangeEvent : UnityEngine.Events.UnityEvent<Marks> { }

        public const uint MarkNumColors = 216;
        public const uint MarkAim = MarkNumColors + 1;
        public const uint MarkAimAttack = MarkNumColors + 2;
        public const uint MarkAimFollow = MarkNumColors + 3;
        public const uint MarkAttack = MarkNumColors + 4;
        public const uint MarkFollow = MarkNumColors + 5;
        public const uint MarksNumTotal = MarkFollow + 1;
        public const uint MarkUnmarked = 255;

        public MarksChangeEvent onMarksChange = new MarksChangeEvent();

        private Dictionary<MarkTypes, MarkBase> m_CurrentMarks;

        public Dictionary<MarkTypes, MarkBase> CurrentMarks { get => m_CurrentMarks; }

        public uint GetMarkColor(MarkTypes markTypes) {
            if (m_CurrentMarks != null && m_CurrentMarks.TryGetValue(markTypes, out MarkBase markBase))
                return markBase.MarkColor;

            return MarkUnmarked;
        }

        public bool AreAnyMarksSet(IEnumerable<MarkTypes> markTypes) {
            if (m_CurrentMarks == null)
                return false;
            
            foreach (MarkTypes markType in markTypes) {
                if (IsMarkSet(markType))
                    return true;
            }

            return false;
        }

        public bool AreAllMarksSet(IEnumerable<MarkTypes> markTypes) {
            if (m_CurrentMarks == null)
                return false;

            foreach (MarkTypes markType in markTypes) {
                if (!IsMarkSet(markType))
                    return false;
            }

            return true;
        }

        public bool IsMarkSet(MarkTypes markType) {
            if (m_CurrentMarks == null)
                return false;

            MarkBase markBase;
            if (m_CurrentMarks.TryGetValue(markType, out markBase)) {
                return markBase.IsSet;
            }

            return false;
        }

        public void Clear() {
            if (m_CurrentMarks != null && m_CurrentMarks.Count > 0) {
                m_CurrentMarks.Clear();

                onMarksChange.Invoke(this);
            }
        }

        public void SetMark(MarkTypes markType, uint color) {
            if (m_CurrentMarks == null) {
                if (color == MarkUnmarked)
                    return;
                else
                    m_CurrentMarks = new Dictionary<MarkTypes, MarkBase>();
            }

            MarkBase _;
            if (!m_CurrentMarks.TryGetValue(markType, out _)) {
                if (markType == MarkTypes.OneSecondTemp)
                    m_CurrentMarks.Add(MarkTypes.OneSecondTemp, new MarkTimeOut(1000));
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

    public class MarkBase
    {
        protected uint m_MarkColor = 0;
        public uint MarkColor { get { return m_MarkColor; } }

        public virtual void Set(uint color) {
            m_MarkColor = color;
        }

        public virtual bool IsSet {
            get {
                return m_MarkColor != Marks.MarkUnmarked;
            }
        }
    }

    public class MarkTimeOut : MarkBase
    {
        System.Timers.Timer m_Timer;
        int m_TimeoutMilliseconds;
        int m_SetTime;

        public MarkTimeOut(int millis) {
            m_TimeoutMilliseconds = millis;
        }

        public override void Set(uint color) {
            m_MarkColor = 0;
            m_SetTime = OpenTibiaUnity.TicksMillis;
        }

        public override bool IsSet {
            get {
                return base.IsSet && m_SetTime + m_TimeoutMilliseconds > OpenTibiaUnity.TicksMillis;
            }
        }
    }
}
