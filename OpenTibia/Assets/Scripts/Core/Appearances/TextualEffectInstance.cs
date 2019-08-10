using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    internal class TextualEffectInstance : AppearanceInstance
    {
        private const int PhaseDuration = 100;
        private const int PhaseCount = 10;

        private int m_Phase = 0;
        private int m_LastPhaseChange;

        private bool m_Dirty = false;
        private TMPro.TextMeshProUGUI m_TextMesh = null;
        private Vector2 m_Size = Vector2.zero;

        internal bool Visible {
            get { return m_TextMesh != null ? m_TextMesh.gameObject.activeSelf : false; }
            set { if (m_TextMesh) m_TextMesh.gameObject.SetActive(value); }
        }

        internal float Width {
            get {
                CheckDirty();
                return m_Size.x;
            }
        }

        internal float Height {
            get {
                CheckDirty();
                return m_Size.y;
            }
        }

        internal override int Phase {
            get {
                return m_Phase;
            }
        }
        
        internal TextualEffectInstance(int color, string text, TMPro.TextMeshProUGUI textMesh) : base(0, null) {
            m_LastPhaseChange = OpenTibiaUnity.TicksMillis;
            m_TextMesh = textMesh;
            m_TextMesh.text = text;
            m_TextMesh.color = Colors.ColorFrom8Bit(color);
            m_Size = m_TextMesh.GetPreferredValues();
            m_Dirty = false;
        }

        internal bool Merge(AppearanceInstance other) {
            var textualEffect = other as TextualEffectInstance;
            if (!!textualEffect && textualEffect.m_Phase <= 0 && m_Phase <= 0 && textualEffect.m_TextMesh.color != m_TextMesh.color) {
                m_TextMesh.text = textualEffect.m_TextMesh.text;
                m_Dirty = true;
                return true;
            }

            return false;
        }

        internal override bool Animate(int ticks, int delay = 0) {
            var timeChange = Mathf.Abs(ticks - m_LastPhaseChange);
            int phaseChange = timeChange / PhaseDuration;
            m_Phase += phaseChange;
            m_LastPhaseChange += phaseChange * PhaseDuration;
            return m_Phase < PhaseCount;
        }

        internal void DestroyTextMesh() {
            if (m_TextMesh) {
                Object.Destroy(m_TextMesh.gameObject);
                m_TextMesh = null;
            }
        }

        private void CheckDirty() {
            if (!m_Dirty)
                return;

            m_Size = m_TextMesh.GetPreferredValues();
            m_Dirty = false;
        }

        internal void UpdateTextMeshPosition(float x, float y) {
            var rectTransform = m_TextMesh.rectTransform;

            float width = Mathf.Min(m_TextMesh.preferredWidth, Constants.OnscreenMessageWidth);
            float height = m_TextMesh.preferredHeight;

            var parentRT = rectTransform.parent as RectTransform;

            x = Mathf.Clamp(x, width / 2, parentRT.rect.width - width / 2);
            y = Mathf.Clamp(y, -parentRT.rect.height + height / 2, -height / 2);

            if (rectTransform.anchoredPosition.x == x && rectTransform.anchoredPosition.y == y)
                return;

            rectTransform.anchoredPosition = new Vector3(x, y, 0);
        }
    }
}
