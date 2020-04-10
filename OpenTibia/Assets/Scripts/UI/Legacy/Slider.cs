using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class Slider : UnityUI.Slider
    {
        public TMPro.TextMeshProUGUI label = null;
        public bool UseIntegralValues = true;

        public Color fillColor {
            set {
                var rawImage = fillRect.GetComponent<UnityUI.RawImage>();
                if (rawImage != null) {
                    rawImage.color = value;
                    return;
                }

                // fallback to image
                var image = fillRect.GetComponent<UnityUI.Image>();
                if (image != null)
                    image.color = value;
            }
        }

        protected override void Awake() {
            base.Awake();

            onValueChanged.AddListener(OnSliderValueChanged);
        }

        public void SetMinMax(float min, float max) {
            minValue = min;
            maxValue = max;
        }

        public void SetEnabled(bool enabled) {
            if (enabled)
                EnableComponent();
            else
                DisableComponent();
        }

        public void DisableComponent() {
            interactable = false;
            if (label)
                label.color = Core.Colors.DefaultDisabled;
        }

        public void EnableComponent() {
            interactable = true;
            if (label)
                label.color = Core.Colors.Default;
        }

        private void OnSliderValueChanged(float value) {
            ForceUpdateLabel();
        }

        public void ForceUpdateLabel() {
            if (label) {
                if (UseIntegralValues)
                    label.text = ((int)value).ToString();
                else
                    label.text = value.ToString();
            }
        }
    }
}
