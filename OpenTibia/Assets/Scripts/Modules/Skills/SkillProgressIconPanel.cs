using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Skills
{
    public class SkillProgressIconPanel : SkillPanel
    {
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelText = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelValue = null;
        [SerializeField] private Image m_IconImage = null;
        [SerializeField] private Slider m_ProgressBar = null;
        [SerializeField] private RawImage m_FillAreaImage = null;

        internal override TMPro.TextMeshProUGUI labelText { get => m_LabelText; }
        internal override TMPro.TextMeshProUGUI labelValue { get => m_LabelValue; }

        protected override void Start() {
            base.Start();

            m_ProgressBar.minValue = 0;
            m_ProgressBar.maxValue = 100;
        }

        public override void SetProgressColor(Color color) => m_FillAreaImage.color = color;
        public override void SetIcon(Sprite sprite) => m_IconImage.sprite = sprite;
        public override void SetText(string text) => m_LabelText.SetText(text);
        public override void SetValue(long value) => SetValue(value, 0);
        public override void SetValue(long value, float percent) => SetValue(Utility.Commafy(value), percent);

        private void SetValue(string value, float percent) {
            m_LabelValue.SetText(value);
            m_ProgressBar.value = percent;
        }
    }
}
