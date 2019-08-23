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

        private RawImage _rawImage;
        public RawImage rawImage {
            get {
                if (!_rawImage)
                    _rawImage = GetComponent<RawImage>();
                return _rawImage;
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
