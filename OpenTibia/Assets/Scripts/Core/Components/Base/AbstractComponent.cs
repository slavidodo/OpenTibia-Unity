using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Components.Base
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class AbstractComponent : UIBehaviour
    {
        public static AbstractComponent TopLockedComponent = null;
        public static List<AbstractComponent> QueuedLockComponents = new List<AbstractComponent>();

        public static int BlockerIndexCounter = 20000;

        private RectTransform _rectTransform;
        public RectTransform rectTransform {
            get {
                if (!_rectTransform)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        private RectTransform _parentRectTransform;
        public RectTransform parentRectTransform {
            get {
                if (!_parentRectTransform)
                    _parentRectTransform = transform.parent as RectTransform;
                return _parentRectTransform;
            }
        }

        public void ClampToParent() {
            rectTransform.localPosition = ClampLocalPositionToParent(rectTransform.localPosition);
        }

        public Vector3 ClampLocalPositionToParent(Vector2 localPosition) {
            Vector3 minPosition = parentRectTransform.rect.min - rectTransform.rect.min;
            Vector3 maxPosition = parentRectTransform.rect.max - rectTransform.rect.max;

            localPosition.x = Mathf.Clamp(localPosition.x, minPosition.x, maxPosition.x);
            localPosition.y = Mathf.Clamp(localPosition.y, minPosition.y, maxPosition.y);
            return localPosition;
        }

        public virtual Vector2 CalculateAbsoluteMousePosition() {
            return CalculateAbsoluteMousePosition(UnityEngine.Input.mousePosition);
        }

        public virtual Vector2 CalculateAbsoluteMousePosition(Vector2 mousePosition) {
            var pivotDelta = rectTransform.pivot - new Vector2(0, 1);
            var size = rectTransform.rect.size;
            return mousePosition + pivotDelta * size;
        }

        public virtual void Select() {
            if (EventSystem.current.alreadySelecting) {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(Select);
                return;
            }

            if (EventSystem.current.currentSelectedGameObject == gameObject)
                return;

            OpenTibiaUnity.EventSystem.SetSelectedGameObject(gameObject);
        }
    }
}