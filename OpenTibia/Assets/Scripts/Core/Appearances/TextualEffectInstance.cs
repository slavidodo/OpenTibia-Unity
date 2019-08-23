using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class TextualEffectInstance : AppearanceInstance
    {
        private const int PhaseDuration = 100;
        private const int PhaseCount = 10;

        private int _phase = 0;
        private int _lastPhaseChange;

        private bool _dirty = false;
        private TMPro.TextMeshProUGUI _textMesh = null;
        private Vector2 _size = Vector2.zero;

        public bool Visible {
            get { return _textMesh != null ? _textMesh.gameObject.activeSelf : false; }
            set { if (_textMesh) _textMesh.gameObject.SetActive(value); }
        }

        public float Width {
            get {
                CheckDirty();
                return _size.x;
            }
        }

        public float Height {
            get {
                CheckDirty();
                return _size.y;
            }
        }

        public override int Phase {
            get {
                return _phase;
            }
        }
        
        public TextualEffectInstance(int color, string text, TMPro.TextMeshProUGUI textMesh) : base(0, null) {
            _lastPhaseChange = OpenTibiaUnity.TicksMillis;
            _textMesh = textMesh;
            _textMesh.text = text;
            _textMesh.color = Colors.ColorFrom8Bit(color);
            _size = _textMesh.GetPreferredValues();
            _dirty = false;
        }

        public bool Merge(AppearanceInstance other) {
            var textualEffect = other as TextualEffectInstance;
            if (!!textualEffect && textualEffect._phase <= 0 && _phase <= 0 && textualEffect._textMesh.color != _textMesh.color) {
                _textMesh.text = textualEffect._textMesh.text;
                _dirty = true;
                return true;
            }

            return false;
        }

        public override bool Animate(int ticks, int delay = 0) {
            var timeChange = Mathf.Abs(ticks - _lastPhaseChange);
            int phaseChange = timeChange / PhaseDuration;
            _phase += phaseChange;
            _lastPhaseChange += phaseChange * PhaseDuration;
            return _phase < PhaseCount;
        }

        public void DestroyTextMesh() {
            if (_textMesh) {
                Object.Destroy(_textMesh.gameObject);
                _textMesh = null;
            }
        }

        private void CheckDirty() {
            if (!_dirty)
                return;

            _size = _textMesh.GetPreferredValues();
            _dirty = false;
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
