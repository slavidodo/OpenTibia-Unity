using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Draggable))]
    public class DraggableTabButton : TabButton, IDragHandler, IEndDragHandler
    {
        Draggable _draggableComponent;
        public Draggable draggableComponent {
            get {
                if (!_draggableComponent)
                    _draggableComponent = GetComponent<Draggable>();
                return _draggableComponent;
            }
        }

        GameObject _shadowGameObject = null;

        public void OnDrag(PointerEventData eventData) {
            if (!_shadowGameObject) {
                InstantiateShadowGameObject(transform.GetSiblingIndex());
                transform.SetAsLastSibling();
                layoutElement.ignoreLayout = true;
            }

            Invoke("UpdateSiblingIndex", 0);
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!!_shadowGameObject) {
                layoutElement.ignoreLayout = false;
                transform.SetSiblingIndex(_shadowGameObject.transform.GetSiblingIndex());
                Destroy(_shadowGameObject);
            }
        }

        protected void InstantiateShadowGameObject(int siblingIndex) {
            _shadowGameObject = Instantiate(gameObject, transform.parent);
            Destroy(_shadowGameObject.GetComponent<TabButton>()); // destroy tab button component
            Destroy(_shadowGameObject.GetComponent<Selectable>()); // destroy any selectable component
            Destroy(_shadowGameObject.GetComponent<Graphic>()); // destroy any graphic component

            var components = _shadowGameObject.GetComponentsInChildren<Graphic>();
            foreach (var component in components)
                Destroy(component);

            _shadowGameObject.transform.SetSiblingIndex(siblingIndex);
        }

        protected void UpdateSiblingIndex() {
            int currentIndex = _shadowGameObject.transform.GetSiblingIndex();
            var localPosition = transform.localPosition;
            
            int childCount = parentRectTransform.childCount;
            int foundIndex = -1;

            // check for next elements (starting from the end)
            for (int i = childCount - 1; i > currentIndex; i--) {
                var child = parentRectTransform.GetChild(i) as RectTransform;
                if (localPosition.x > child.localPosition.x + child.rect.size.x / 3f) {
                    foundIndex = i;
                    break;
                }
            }
            
            if (foundIndex != -1 && foundIndex != currentIndex) {
                _shadowGameObject.transform.SetSiblingIndex(foundIndex);
                return;
            }

            // check for previous elements (starting from the begin)
            for (int i = 0; i < currentIndex; i++) {
                var child = parentRectTransform.GetChild(i) as RectTransform;
                if (localPosition.x < child.localPosition.x + child.rect.size.x / 1.5f) {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1 && foundIndex != currentIndex) {
                _shadowGameObject.transform.SetSiblingIndex(foundIndex);
                return;
            }
        }
    }
}
