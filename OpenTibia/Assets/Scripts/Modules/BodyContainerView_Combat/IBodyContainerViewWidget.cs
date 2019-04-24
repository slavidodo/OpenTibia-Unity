using OpenTibiaUnity.Core.Appearances;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.BodyContainerView_Combat
{
    public interface IBodyContainerViewWidget
    {
        void OnInventorySlotChange(ClothSlots slot, ObjectInstance obj);
    }
}
