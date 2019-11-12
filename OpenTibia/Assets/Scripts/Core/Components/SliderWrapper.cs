using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Slider))]
    public class SliderWrapper : Base.AbstractComponent
    {
        private Slider _slider;
        public Slider slider {
            get {
                if (!_slider)
                    _slider = GetComponent<Slider>();
                return _slider;
            }
        }

        public TMPro.TextMeshProUGUI label = null;
        public bool UseIntegralValues = true;

        public Slider.SliderEvent onValueChanged { get => slider.onValueChanged; }

        protected override void Awake() {
            base.Awake();

            onValueChanged.AddListener(OnSliderValueChanged);
        }

        public void SetMinMax(float min, float max) {
            slider.minValue = min;
            slider.maxValue = max;

            float pxrange;
            if (slider.direction == Slider.Direction.BottomToTop || slider.direction == Slider.Direction.TopToBottom)
                pxrange = rectTransform.rect.height - 24;
            else
                pxrange = rectTransform.rect.width - 24;

            float range = slider.maxValue - slider.minValue + 1;
            float proportion = Mathf.Clamp(14, 1, range) / range;

            float px = Mathf.Max(proportion * pxrange, 6);
            px = px - px % 2 + 1;
        }

        public void SetEnabled(bool enabled) {
            if (enabled)
                EnableComponent();
            else
                DisableComponent();
        }

        public void DisableComponent() {
            slider.interactable = false;
            if (label)
                label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        public void EnableComponent() {
            slider.interactable = true;
            if (label)
                label.color = Colors.ColorFromRGB(0xC0C0C0);
        }

        private void OnSliderValueChanged(float value) {
            ForceUpdateLabel();
        }

        public void ForceUpdateLabel() {
            if (label) {
                if (UseIntegralValues)
                    label.text = ((int)slider.value).ToString();
                else
                    label.text = slider.value.ToString();
            }
        }
    }
}
