using OpenTibiaUnity.Core.Appearances;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Container
{
    internal class BodyContainerView
    {
        internal class ObjectChangeEvent : UnityEvent<ClothSlots, ObjectInstance> { }

        internal ObjectChangeEvent onSlotChange = new ObjectChangeEvent();
        internal UnityEvent onReset = new UnityEvent();

        internal ObjectInstance[] Objects { get; private set; }
        
        internal BodyContainerView() {
            Objects = new ObjectInstance[ClothSlots.Last - ClothSlots.First + 1];
        }

        internal void SetObject(ClothSlots slot, ObjectInstance @object) {
            if (slot < ClothSlots.First || slot > ClothSlots.Last)
                throw new System.IndexOutOfRangeException("BodyContainerView.getObject: Index out of range: " + slot);

            Objects[slot - ClothSlots.First] = @object;
            onSlotChange.Invoke(slot, @object);
        }

        internal ObjectInstance GetObject(ClothSlots slot) {
            if (slot < ClothSlots.First || slot > ClothSlots.Last)
                throw new System.IndexOutOfRangeException("BodyContainerView.getObject: Index out of range: " + slot);

            return Objects[slot - ClothSlots.First];
        }

        internal bool IsEquipped(uint objectID) {
            var appearanceStorage = OpenTibiaUnity.AppearanceStorage;
            var appearanceType = appearanceStorage.GetObjectType(objectID);
            if (!!appearanceType && appearanceType.IsCloth) {
                ClothSlots clothSlot = (ClothSlots)appearanceType.Cloth;
                if (clothSlot == ClothSlots.BothHands)
                    clothSlot = ClothSlots.LeftHand;
                var @object = Objects[clothSlot - ClothSlots.First];
                if (!!@object)
                    return true;
            }

            return false;
        }

        internal void Reset() {
            for (var i = ClothSlots.First; i <= ClothSlots.Last; i++)
                Objects[i - ClothSlots.First] = null;

            onReset.Invoke();
        }
    }
}
