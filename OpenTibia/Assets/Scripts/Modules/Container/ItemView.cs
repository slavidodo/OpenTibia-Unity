using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Container
{
    internal class ItemViewPointerEvent : UnityEvent<ClothSlots> { }

    internal class ItemView : Core.Components.Base.AbstractComponent, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RawImage m_ItemImage = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_ItemText = null;
        [SerializeField] private bool m_ShowAmount = false;
        [SerializeField] private int m_ObjectAmount = 1;
        [SerializeField] private ClothSlots m_ClothSlot = ClothSlots.Head;

        internal ItemViewPointerEvent onPointerEnter;
        internal ItemViewPointerEvent onPointerExit;

        internal RawImage itemImage { get => m_ItemImage; }
        internal TMPro.TextMeshProUGUI itemText { get => m_ItemText; }
        
        internal bool showAmount {
            get => m_ShowAmount;
            set {
                if (m_ShowAmount != value) {
                    m_ShowAmount = value;
                    itemText.gameObject.SetActive(m_ShowAmount);
                }
            }
        }
        internal int objectAmount {
            get => m_ObjectAmount;
            set {
                if (m_ObjectAmount != value) {
                    m_ObjectAmount = value;
                    itemText.SetText(m_ObjectAmount.ToString());

                    if (!!m_ObjectInstance)
                        m_ObjectInstance.Data = (uint)m_ObjectAmount;
                }
            }
        }
        internal ClothSlots clothSlot { get => m_ClothSlot; }

        private Core.Appearances.ObjectInstance m_ObjectInstance = null;
        internal Core.Appearances.ObjectInstance objectInstance {
            get => m_ObjectInstance;
            set => m_ObjectInstance = value;
        }

        protected override void Awake() {
            base.Awake();

            onPointerEnter = new ItemViewPointerEvent();
            onPointerExit = new ItemViewPointerEvent();
        }

        protected override void Start() {
            base.Start();
            
            itemText.gameObject.SetActive(m_ShowAmount);
            itemText.SetText(m_ObjectAmount.ToString());
        }

        public void OnPointerEnter(PointerEventData eventData) {
            onPointerEnter.Invoke(m_ClothSlot);
        }

        public void OnPointerExit(PointerEventData eventData) {
            onPointerExit.Invoke(m_ClothSlot);
        }
    }
}
