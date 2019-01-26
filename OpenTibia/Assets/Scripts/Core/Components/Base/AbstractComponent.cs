using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Components.Base
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class AbstractComponent : UIBehaviour
    {
        private RectTransform m_RectTransform;
        public RectTransform rectTransform {
            get {
                if (!m_RectTransform)
                    m_RectTransform = transform as RectTransform;
                return m_RectTransform;
            }
        }

        private RectTransform m_ParentRectTransform;
        public RectTransform parentRectTransform {
            get {
                if (!m_ParentRectTransform)
                    m_ParentRectTransform = transform.parent as RectTransform;
                return m_ParentRectTransform;
            }
        }

        internal protected void ClampToParent() {
            rectTransform.localPosition = ClampLocalPositionToParent(rectTransform.localPosition);
        }

        public Vector3 ClampLocalPositionToParent(Vector2 localPosition) {
            Vector3 minPosition = parentRectTransform.rect.min - rectTransform.rect.min;
            Vector3 maxPosition = parentRectTransform.rect.max - rectTransform.rect.max;

            localPosition.x = Mathf.Clamp(rectTransform.localPosition.x, minPosition.x, maxPosition.x);
            localPosition.y = Mathf.Clamp(rectTransform.localPosition.y, minPosition.y, maxPosition.y);

            return localPosition;
        }
    }
}