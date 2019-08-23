using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    public sealed class LegacyOptionsWindowItem : AbstractComponent
    {
        public ButtonWrapper buttonWrapper;
        public TMPro.TextMeshProUGUI label;
    }
}
