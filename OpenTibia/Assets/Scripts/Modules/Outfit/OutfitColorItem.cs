using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Outfit
{
    internal class OutfitColorItem : Core.Components.Base.AbstractComponent
    {
        [SerializeField] internal Toggle toggleComponent = null;
        [SerializeField] internal Image imageComponent = null;
    }
}
