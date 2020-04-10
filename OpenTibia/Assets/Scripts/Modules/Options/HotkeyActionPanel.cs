using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    public class HotkeyActionPanel : UI.Legacy.ToggleListItem
    {
        // serialized fields
        [SerializeField]
        private TMPro.TextMeshProUGUI _textComponent = null;

        // properties
        public KeyCode KeyCode { get; set; }
        public EventModifiers EventModifiers { get; set; }
        public string BaseText { get; set; } = string.Empty;

        public string text {
            get => _textComponent.text;
            set => _textComponent.text = value;
        }

        public Color textColor {
            get => _textComponent.color;
            set => _textComponent.color = value;
        }
    }
}
