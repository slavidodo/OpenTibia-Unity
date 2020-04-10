using OpenTibiaUnity.Core.Components.Base;
using UnityEngine;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    public sealed class LegacyOptionsWidgetItem : AbstractComponent
    {
        [SerializeField]
        private UI.Legacy.Button _button = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _label = null;

        public UI.Legacy.Button button { get => _button; }
        public string text {
            get => _label.text;
            set => _label.text = value;
        }
    }
}
