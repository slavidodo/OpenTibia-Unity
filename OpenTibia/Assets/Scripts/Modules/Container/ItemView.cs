using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Container
{
    public class ItemViewPointerEvent : UnityEvent<ClothSlots> { }

    public class ItemView : Core.Components.Base.AbstractComponent, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RawImage _itemImage = null;
        [SerializeField] private TMPro.TextMeshProUGUI _itemText = null;
        [SerializeField] private bool _showAmount = false;
        [SerializeField] private int _objectAmount = 1;
        [SerializeField] private ClothSlots _clothSlot = ClothSlots.Head;

        public ItemViewPointerEvent onPointerEnter;
        public ItemViewPointerEvent onPointerExit;

        public RawImage itemImage { get => _itemImage; }
        public TMPro.TextMeshProUGUI itemText { get => _itemText; }
        
        public bool showAmount {
            get => _showAmount;
            set {
                if (_showAmount != value) {
                    _showAmount = value;
                    itemText.gameObject.SetActive(_showAmount);
                }
            }
        }
        public int objectAmount {
            get => _objectAmount;
            set {
                if (_objectAmount != value) {
                    _objectAmount = value;
                    itemText.SetText(_objectAmount.ToString());

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

        protected override void Awake() {
            base.Awake();

            onPointerEnter = new ItemViewPointerEvent();
            onPointerExit = new ItemViewPointerEvent();
        }

        protected override void Start() {
            base.Start();
            
            itemText.gameObject.SetActive(_showAmount);
            itemText.SetText(_objectAmount.ToString());
        }

        public void OnPointerEnter(PointerEventData eventData) {
            onPointerEnter.Invoke(_clothSlot);
        }

        public void OnPointerExit(PointerEventData eventData) {
            onPointerExit.Invoke(_clothSlot);
        }
    }
}
