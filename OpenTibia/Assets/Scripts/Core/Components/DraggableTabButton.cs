using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Draggable))]
    public class DraggableTabButton : TabButton, IDragHandler, IEndDragHandler
    {
        Draggable m_DraggableComponent;
        public Draggable draggableComponent {
            get {
                if (!m_DraggableComponent)
                    m_DraggableComponent = GetComponent<Draggable>();
                return m_DraggableComponent;
            }
        }

        GameObject m_ShadowGameObject = null;

        public void OnDrag(PointerEventData eventData) {
            if (!m_ShadowGameObject) {
                InstantiateShadowGameObject(transform.GetSiblingIndex());
                transform.SetAsLastSibling();
                layoutElement.ignoreLayout = true;
            }

            Invoke("UpdateSiblingIndex", 0);
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (!!m_ShadowGameObject) {
                layoutElement.ignoreLayout = false;
                transform.SetSiblingIndex(m_ShadowGameObject.transform.GetSiblingIndex());
                Destroy(m_ShadowGameObject);
            }
        }

        protected void InstantiateShadowGameObject(int siblingIndex) {
            m_ShadowGameObject = Instantiate(gameObject, transform.parent);
            Destroy(m_ShadowGameObject.GetComponent<TabButton>()); // destroy tab button component
            Destroy(m_ShadowGameObject.GetComponent<Selectable>()); // destroy any selectable component
            Destroy(m_ShadowGameObject.GetComponent<Graphic>()); // destroy any graphic component

            var components = m_ShadowGameObject.GetComponentsInChildren<Graphic>();
            foreach (var component in components)
                Destroy(component);

            m_ShadowGameObject.transform.SetSiblingIndex(siblingIndex);
        }

        protected void UpdateSiblingIndex() {
            int currentIndex = m_ShadowGameObject.transform.GetSiblingIndex();
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
                m_ShadowGameObject.transform.SetSiblingIndex(foundIndex);
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
                m_ShadowGameObject.transform.SetSiblingIndex(foundIndex);
                return;
            }
        }
    }
}
