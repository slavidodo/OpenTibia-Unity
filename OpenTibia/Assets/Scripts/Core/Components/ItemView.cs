using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class UnityClothChangeEvent : UnityEvent<ClothSlots> { }

    public class ItemView : Base.AbstractComponent, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RawImage m_ItemImage;
        [SerializeField] private TMPro.TextMeshProUGUI m_ItemText;
        [SerializeField] private bool m_ShowAmount = false;
        [SerializeField] private int m_ObjectAmount = 1;
        [SerializeField] private ClothSlots m_ClothSlot;
#pragma warning restore CS0649 // never assigned to

        public UnityClothChangeEvent onPointerEnter;
        public UnityClothChangeEvent onPointerExit;

        public RawImage itemImage { get => m_ItemImage; }
        public TMPro.TextMeshProUGUI itemText { get => m_ItemText; }
        
        public bool showAmount {
            get => m_ShowAmount;
            set {
                if (m_ShowAmount != value) {
                    m_ShowAmount = value;
                    itemText.gameObject.SetActive(m_ShowAmount);
                }
            }
        }
        public int objectAmount {
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
        public ClothSlots clothSlot { get => m_ClothSlot; }

        private Appearances.ObjectInstance m_ObjectInstance = null;
        public Appearances.ObjectInstance objectInstance {
            get => m_ObjectInstance;
            set => m_ObjectInstance = value;
        }

        protected override void Awake() {
            base.Awake();

            onPointerEnter = new UnityClothChangeEvent();
            onPointerExit = new UnityClothChangeEvent();
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
