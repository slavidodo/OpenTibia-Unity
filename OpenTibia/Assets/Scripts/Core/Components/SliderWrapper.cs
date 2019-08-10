using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Slider))]
    public class SliderWrapper : Base.AbstractComponent
    {
        private Slider m_Slider;
        public Slider slider {
            get {
                if (!m_Slider)
                    m_Slider = GetComponent<Slider>();
                return m_Slider;
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
