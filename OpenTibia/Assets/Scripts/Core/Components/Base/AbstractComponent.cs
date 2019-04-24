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
        
        public bool LockedToOverlay { get; private set; } = false;
        public Canvas LockingBlocker { get; private set; } = null;

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

        public void LockToOverlay() {
            // is this component locked already?
            if (LockedToOverlay) {
                // is this component the current locked one?
                if (TopLockedComponent == this)
                    return;
                else if (TopLockedComponent != null)
                    QueuedLockComponents.Add(TopLockedComponent);
                // it's locked but must re.init
                QueuedLockComponents.Remove(this);
            } else if (TopLockedComponent != null) {
                // a component is locked already, push that to the stack and lock the new one.
                QueuedLockComponents.Add(TopLockedComponent);
            }

            // lock this component to overlay
            InternalLockToOverlay();
        }

        private void InternalLockToOverlay() {
            var gameManager = OpenTibiaUnity.GameManager;
            if (TopLockedComponent != null) {
                if (TopLockedComponent.LockingBlocker != gameManager.ActiveBlocker) {
                    TopLockedComponent.LockingBlocker.gameObject.SetActive(false);
                    gameManager.ActiveBlocker.gameObject.SetActive(true);
                }
            } else {
                gameManager.ActiveBlocker.gameObject.SetActive(true);
            }

            gameManager.ActiveBlocker.transform.SetAsLastSibling();
            transform.SetParent(gameManager.ActiveCanvas.transform);
            transform.SetAsLastSibling();

            var canvas = transform.GetComponent<Canvas>();
            if (canvas != null) {
                canvas.sortingOrder = ++BlockerIndexCounter;
            }

            AbstractComponent.TopLockedComponent = this;
            this.LockedToOverlay = true;
            this.LockingBlocker = gameManager.ActiveBlocker;
        }

        public void UnlockFromOverlay() {
            if (!LockedToOverlay)
                return;

            var gameManager = OpenTibiaUnity.GameManager;
            if (QueuedLockComponents.Count == 0) {
                // no more components to lock, restore overlay.
                LockingBlocker.gameObject.SetActive(false);
                TopLockedComponent = null;
            } else {
                var component = QueuedLockComponents[0];
                QueuedLockComponents.RemoveAt(0);
                component.InternalLockToOverlay();
            }

            LockedToOverlay = false;
            LockingBlocker = null;
        }

        public void ResetLocalPosition() {
            GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        }
    }
}