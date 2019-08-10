using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    internal class CheckboxWrapper : Base.AbstractComponent
    {
        internal class ValueChangedEvent : UnityEvent<bool> { }

        [SerializeField] internal Checkbox checkbox = null;
        [SerializeField] internal TMPro.TextMeshProUGUI label = null;

        internal Toggle.ToggleEvent onValueChanged { get => checkbox.toggleComponent.onValueChanged; }
        
        internal void SetEnabled(bool enabled) {
            if (enabled)
                EnableComponent();
            else
                DisableComponent();
        }

        internal void DisableComponent() {
            checkbox.DisableToggle();
            label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        internal void EnableComponent() {
            checkbox.EnableToggle();
            label.color = Colors.ColorFromRGB(0xC0C0C0);
        }

        private void OnCheckboxValueChanged(bool value) {
            onValueChanged.Invoke(value);
        }
    }
}
