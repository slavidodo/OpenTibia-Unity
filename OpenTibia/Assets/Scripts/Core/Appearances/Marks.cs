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

        private Dictionary<MarkType, MarkBase> _currentMarks;

        public Dictionary<MarkType, MarkBase> CurrentMarks { get => _currentMarks; }

        public uint GetMarkColor(MarkType markTypes) {
            if (_currentMarks != null && _currentMarks.TryGetValue(markTypes, out MarkBase markBase))
                return markBase.MarkColor;

            return MarkUnmarked;
        }

        public bool AnyMarkSet() {
            return _currentMarks != null && _currentMarks.Count > 0;
        }

        public bool AreAnyMarksSet(IEnumerable<MarkType> markTypes) {
            if (_currentMarks == null)
                return false;
            
            foreach (MarkType markType in markTypes) {
                if (IsMarkSet(markType))
                    return true;
            }

            return false;
        }

        public bool AreAllMarksSet(IEnumerable<MarkType> markTypes) {
            if (_currentMarks == null)
                return false;

            foreach (MarkType markType in markTypes) {
                if (!IsMarkSet(markType))
                    return false;
            }

            return true;
        }

        public bool IsMarkSet(MarkType markType) {
            if (_currentMarks == null)
                return false;

            MarkBase markBase;
            if (_currentMarks.TryGetValue(markType, out markBase)) {
                return markBase.IsSet;
            }

            return false;
        }

        public void Clear() {
            if (_currentMarks != null && _currentMarks.Count > 0) {
                _currentMarks.Clear();

                onMarksChange.Invoke(this);
            }
        }

        public void SetMark(MarkType markType, uint color) {
            if (_currentMarks == null) {
                if (color == MarkUnmarked)
                    return;
                else
                    _currentMarks = new Dictionary<MarkType, MarkBase>();
            }

            MarkBase _;
            if (!_currentMarks.TryGetValue(markType, out _)) {
                if (markType == MarkType.OneSecondTemp)
                    _currentMarks.Add(MarkType.OneSecondTemp, new MarkTimeOut(1000));
                else
                    _currentMarks.Add(markType, new MarkBase());
            }

            if (GetMarkColor(markType) != color) {
                _currentMarks[markType].Set(color);

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
        protected uint _markColor = 0;
        public uint MarkColor { get { return _markColor; } }

        public virtual void Set(uint color) {
            _markColor = color;
        }

        public virtual bool IsSet {
            get {
                return _markColor != Marks.MarkUnmarked;
            }
        }
    }

    public class MarkTimeOut : MarkBase
    {
        System.Timers.Timer _timer;
        int _timeoutMilliseconds;
        int _setTime;

        public MarkTimeOut(int millis) {
            _timeoutMilliseconds = millis;
        }

        public override void Set(uint color) {
            _markColor = 0;
            _setTime = OpenTibiaUnity.TicksMillis;
        }

        public override bool IsSet {
            get {
                return base.IsSet && _setTime + _timeoutMilliseconds > OpenTibiaUnity.TicksMillis;
            }
        }
    }
}
