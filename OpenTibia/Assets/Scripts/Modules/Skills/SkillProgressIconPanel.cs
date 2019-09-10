using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Skills
{
    public class SkillProgressIconPanel : SkillPanel
    {
        [SerializeField] private TMPro.TextMeshProUGUI _labelText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _labelValue = null;
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private Slider _progressBar = null;
        [SerializeField] private RawImage _fillAreaImage = null;

        public override TMPro.TextMeshProUGUI labelText { get => _labelText; }
        public override TMPro.TextMeshProUGUI labelValue { get => _labelValue; }

        protected override void Start() {
            base.Start();

            _progressBar.minValue = 0;
            _progressBar.maxValue = 100;
        }

        public override void SetProgressColor(Color color) => _fillAreaImage.color = color;
        public override void SetIcon(Sprite sprite) => _iconImage.sprite = sprite;
        public override void SetText(string text) => _labelText.SetText(text);
        public override void SetValue(long value) => SetValue(value, 0);
        public override void SetValue(string value) => SetValue(value, 0);
        public override void SetValue(long value, float percent) => SetValueInternal(Core.Utils.Utility.Commafy(value), percent);
        public override void SetValue(string value, float percent) => SetValueInternal(value, percent);

        private void SetValueInternal(string value, float percent) {
            _labelValue.SetText(value);
            _progressBar.value = percent;
        }
    }
}
