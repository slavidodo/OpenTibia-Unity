using OpenTibiaUnity.Core.Appearances;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Container
{
    public class BodyContainerView
    {
        public class ObjectChangeEvent : UnityEvent<ClothSlots, ObjectInstance> { }

        public ObjectChangeEvent onSlotChange = new ObjectChangeEvent();
        public UnityEvent onReset = new UnityEvent();

        public ObjectInstance[] Objects { get; private set; }
        
        public BodyContainerView() {
            Objects = new ObjectInstance[ClothSlots.Last - ClothSlots.First + 1];
        }

        public void SetObject(ClothSlots slot, ObjectInstance @object) {
            if (slot < ClothSlots.First || slot > ClothSlots.Last)
                throw new System.IndexOutOfRangeException("BodyContainerView.getObject: Index out of range: " + slot);

            Objects[slot - ClothSlots.First] = @object;
            onSlotChange.Invoke(slot, @object);
        }

        public ObjectInstance GetObject(ClothSlots slot) {
            if (slot < ClothSlots.First || slot > ClothSlots.Last)
                throw new System.IndexOutOfRangeException("BodyContainerView.getObject: Index out of range: " + slot);

            return Objects[slot - ClothSlots.First];
        }

        public bool IsEquipped(uint objectId) {
            var appearanceStorage = OpenTibiaUnity.AppearanceStorage;
            var appearanceType = appearanceStorage.GetObjectType(objectId);
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

        public void Reset() {
            for (var i = ClothSlots.First; i <= ClothSlots.Last; i++)
                Objects[i - ClothSlots.First] = null;

            onReset.Invoke();
        }
    }
}
