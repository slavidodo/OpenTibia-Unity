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
    }
}
