using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Button))]
    public class ButtonWrapper : Base.AbstractComponent, IPointerDownHandler, IPointerUpHandler
    {
        public TMPro.TextMeshProUGUI label = null;

        private bool _pointerDown = false;

        private Button _button = null;
        public Button button {
            get {
                if (!_button)
                    _button = GetComponent<Button>();
                return _button;
            }
        }
        
        public void DisableComponent() {
            button.interactable = false;
            if (label)
                label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        public void EnableComponent() {
            button.interactable = true;
            if (label)
                label.color = Colors.ColorFromRGB(0xC0C0C0);
        }
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (_pointerDown)
                return;

            _pointerDown = true;
            if (label)
                label.margin += new Vector4(1, -1, 0, 0);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            _pointerDown = false;
            if (label)
                label.margin -= new Vector4(1, -1, 0, 0);
        }
    }
}
