using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class CheckboxPanel : BasicElement
    {
        public Checkbox checkbox = null;
        public TMPro.TextMeshProUGUI label = null;

        public UnityUI.Toggle.ToggleEvent onValueChanged { get => checkbox.toggleComponent.onValueChanged; }

        public void SetEnabled(bool enabled) {
            if (enabled)
                EnableComponent();
            else
                DisableComponent();
        }

        public void DisableComponent() {
            checkbox.DisableToggle();
            if (label)
                label.color = Core.Colors.DefaultDisabled;
        }

        public void EnableComponent() {
            checkbox.EnableToggle();
            if (label)
                label.color = Core.Colors.Default;
        }
    }
}
