using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Components.Base;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    internal sealed class LegacyOptionsWindowItem : AbstractComponent
    {
        [SerializeField] internal ButtonWrapper buttonWrapper;
        [SerializeField] internal TMPro.TextMeshProUGUI label;
    }
}
