using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances
{
    public class Marks
    {
        public static UnityEngine.Color[] s_FrameColors;

        static Marks() {
            s_FrameColors = new UnityEngine.Color[MarksNumTotal];
            for (int i = 0; i < s_FrameColors.Length; i++)
                s_FrameColors[i] = Colors.ColorFrom8Bit(i);

            s_FrameColors[Marks.MarkAim] = Colors.ColorFromRGB(248 << 16 | 248 << 8 | 248);
            s_FrameColors[Marks.MarkAimAttack] = Colors.ColorFromRGB(248 << 16 | 164 << 8 | 164);
            s_FrameColors[Marks.MarkAimFollow] = Colors.ColorFromRGB(180 << 16 | 248 << 8 | 180);
            s_FrameColors[Marks.MarkAttack] = Colors.ColorFromRGB(224 << 16 | 64 << 8 | 64);
            s_FrameColors[Marks.MarkFollow] = Colors.ColorFromRGB(64 << 16 | 224 << 8 | 64);
        }

        public class MarksChangeEvent : UnityEngine.Events.UnityEvent<Marks> { }

        public const int MarkNumColors = 216;
        public const int MarkAim = MarkNumColors + 1;
        public const int MarkAimAttack = MarkNumColors + 2;
        public const int MarkAimFollow = MarkNumColors + 3;
        public const int MarkAttack = MarkNumColors + 4;
        public const int MarkFollow = MarkNumColors + 5;
        public const int MarksNumTotal = MarkFollow + 1;

        public const int MarkType_ClientMapWindow = 1;
        public const int MarkType_ClientBattleList = 2;
        public const int MarkType_OneSecondTemp = 3;
        public const int MarkType_Permenant = 4;

        public const int MarkUnmarked = 255;

        public MarksChangeEvent onMarksChange = new MarksChangeEvent();

        private Dictionary<int, MarkBase> m_CurrentMarks;

        public int GetMarkColor(int type) {
            if (m_CurrentMarks == null) {
                return MarkUnmarked;
            }

            MarkBase markBase;
            if (m_CurrentMarks.TryGetValue(type, out markBase)) {
                return markBase.MarkColor;
            }

            return MarkUnmarked;
        }

        public bool AreAnyMarksSet(IEnumerable<int> marks) {
            if (m_CurrentMarks == null) {
                return false;
            }

            foreach (int mark in marks) {
                if (IsMarkSet(mark)) {
                    return true;
                }
            }

            return false;
        }

        public bool AreAllMarksSet(IEnumerable<int> marks) {
            if (m_CurrentMarks == null) {
                return false;
            }

            foreach (int type in marks) {
                if (!IsMarkSet(type)) {
                    return false;
                }
            }

            return true;
        }

        public bool IsMarkSet(int type) {
            if (m_CurrentMarks == null)
                return false;

            MarkBase markBase;
            if (m_CurrentMarks.TryGetValue(type, out markBase)) {
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

        public void SetMark(int type, int color) {
            if (m_CurrentMarks == null) {
                if (color == MarkUnmarked) {
                    return;
                } else {
                    m_CurrentMarks = new Dictionary<int, MarkBase>();
                }
            }

            MarkBase _;
            if (!m_CurrentMarks.TryGetValue(type, out _)) {
                if (type == MarkType_OneSecondTemp)
                    m_CurrentMarks.Add(MarkType_OneSecondTemp, new MarkTimeOut(1000));
                else
                    m_CurrentMarks.Add(type, new MarkBase());
            }

            if (GetMarkColor(type) != color) {
                m_CurrentMarks[type].Set(color);

                onMarksChange.Invoke(this);
            }
        }

        public void Draw(UnityEngine.Texture2D texture, float rectX, float rectY, UnityEngine.Vector2 zoom, int type) {
            if (m_CurrentMarks == null)
                return;

            MarkBase markBase;
            if (!m_CurrentMarks.TryGetValue(type, out markBase) || markBase.MarkColor == MarkUnmarked)
                return;

            UnityEngine.Rect screenRect = new UnityEngine.Rect() {
                x = rectX * zoom.x,
                y = rectY * zoom.y,
                width = Constants.FieldSize * zoom.x,
                height = Constants.FieldSize * zoom.y
            };

            UnityEngine.Color color = UnityEngine.Color.black;
            if (markBase.MarkColor > 0 && markBase.MarkColor < MarksNumTotal)
                color = s_FrameColors[markBase.MarkColor];

            UnityEngine.Graphics.DrawTexture(screenRect, texture, new UnityEngine.Rect(1f, 1f, 1f, 1f), 0, 0, 0, 0, color);
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
        protected int m_MarkColor = 0;
        public int MarkColor { get { return m_MarkColor; } }

        public virtual void Set(int color) {
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

        public override void Set(int color) {
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
