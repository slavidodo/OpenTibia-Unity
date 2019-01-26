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

        public void SetObject(ClothSlots slot, ObjectInstance obj) {
            if (slot < ClothSlots.First || slot > ClothSlots.Last)
                throw new System.IndexOutOfRangeException("BodyContainerView.getObject: Index out of range: " + slot);

            Objects[slot - ClothSlots.First] = obj;
            onSlotChange.Invoke(slot, obj);
        }

        public ObjectInstance GetObject(ClothSlots slot) {
            if (slot < ClothSlots.First || slot > ClothSlots.Last)
                throw new System.IndexOutOfRangeException("BodyContainerView.getObject: Index out of range: " + slot);

            return Objects[slot - ClothSlots.First];
        }

        public bool IsEquipped(uint objectID) {
            var appearanceStorage = OpenTibiaUnity.AppearanceStorage;
            var appearanceType = appearanceStorage.GetObjectType(objectID);
            if (!!appearanceType && appearanceType.IsCloth) { // TODO; this is not returning the value if the item is two-handed (slot=0)
                var clothSlot = appearanceType.Cloth;
                var obj = Objects[(int)clothSlot - (int)ClothSlots.First];
                if (!!obj)
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
