using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class CheckboxWrapper : Base.AbstractComponent
    {
        public Checkbox checkbox = null;
        public TMPro.TextMeshProUGUI label = null;

        public Toggle.ToggleEvent onValueChanged { get => checkbox.toggleComponent.onValueChanged; }

        public void SetEnabled(bool enabled) {
            if (enabled)
                EnableComponent();
            else
                DisableComponent();
        }

        public void DisableComponent() {
            checkbox.DisableToggle();
            if (label)
                label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        public void EnableComponent() {
            checkbox.EnableToggle();
            if (label)
                label.color = Colors.ColorFromRGB(0xC0C0C0);
        }
    }
}
