using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [RequireComponent(typeof(RawImage))]
    public class GameWorldMap : Core.Components.Base.AbstractComponent, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        private RawImage m_RawImage;
        public RawImage rawImage {
            get {
                if (!m_RawImage)
                    m_RawImage = GetComponent<RawImage>();
                return m_RawImage;
            }
        }

        protected override void Awake() {
            base.Awake();

            onPointerEnter = new UnityEvent();
            onPointerExit = new UnityEvent();
        }

        public void OnPointerEnter(PointerEventData _) {
            onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData _) {
            onPointerExit.Invoke();
        }
    }
}
