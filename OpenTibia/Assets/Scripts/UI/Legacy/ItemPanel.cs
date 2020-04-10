using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class ItemViewPointerEvent : UnityEvent<ClothSlots> { }

    public class ItemPanel : Core.Components.Base.AbstractComponent, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private UnityUI.RawImage _image = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _label = null;
        [SerializeField]
        private bool _showAmount = false;
        [SerializeField]
        private int _objectAmount = 1;
        [SerializeField]
        private ClothSlots _clothSlot = ClothSlots.Head;

        public ItemViewPointerEvent onPointerEnter = new ItemViewPointerEvent();
        public ItemViewPointerEvent onPointerExit = new ItemViewPointerEvent();

        public UnityUI.RawImage image { get => _image; }
        public TMPro.TextMeshProUGUI label { get => _label; }
        
        public bool showAmount {
            get => _showAmount;
            set {
                if (_showAmount != value) {
                    _showAmount = value;
                    label.gameObject.SetActive(_showAmount);
                }
            }
        }

        public int objectAmount {
            get => _objectAmount;
            set {
                if (_objectAmount != value) {
                    _objectAmount = value;
                    label.SetText(_objectAmount.ToString());

                    if (!!_objectInstance)
                        _objectInstance.Data = (uint)_objectAmount;
                }
            }
        }
        public ClothSlots clothSlot { get => _clothSlot; }

        private Core.Appearances.ObjectInstance _objectInstance = null;
        public Core.Appearances.ObjectInstance objectInstance {
            get => _objectInstance;
            set => _objectInstance = value;
        }

        protected override void Start() {
            base.Start();
            
            label.gameObject.SetActive(_showAmount);
            label.SetText(_objectAmount.ToString());
        }

        public void OnPointerEnter(PointerEventData eventData) {
            onPointerEnter.Invoke(_clothSlot);
        }

        public void OnPointerExit(PointerEventData eventData) {
            onPointerExit.Invoke(_clothSlot);
        }
    }
}
