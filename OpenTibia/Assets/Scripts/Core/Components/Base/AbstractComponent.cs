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
                    _rectTransform = transform as RectTransform;
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
                if (gameManager.ActiveBlocker && TopLockedComponent.LockingBlocker != gameManager.ActiveBlocker) {
                    TopLockedComponent.LockingBlocker.gameObject.SetActive(false);
                    gameManager.ActiveBlocker.gameObject.SetActive(true);
                }
            } else if (gameManager.ActiveBlocker) {
                gameManager.ActiveBlocker.gameObject.SetActive(true);
            }

            if (gameManager.ActiveBlocker) {
                gameManager.ActiveBlocker.transform.SetAsLastSibling();
                transform.SetParent(gameManager.ActiveCanvas.transform);
            }

            transform.SetAsLastSibling();

            var canvas = transform.GetComponent<Canvas>();
            if (canvas)
                canvas.sortingOrder = ++BlockerIndexCounter;

            AbstractComponent.TopLockedComponent = this;

            LockedToOverlay = true;
            if (gameManager.ActiveBlocker)
                LockingBlocker = gameManager.ActiveBlocker;
        }

        public void UnlockFromOverlay() {
            if (!LockedToOverlay)
                return;

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

        public virtual Vector2 CalculateRelativeMousePosition() {
            return CalculateRelativeMousePosition(UnityEngine.Input.mousePosition);
        }

        public virtual Vector2 CalculateRelativeMousePosition(Vector3 mousePosition) {
            var pivotDelta = rectTransform.pivot - new Vector2(0, 1);
            var size = rectTransform.rect.size;
            return new Vector2(mousePosition.x + (pivotDelta.x * size.x), mousePosition.y + (pivotDelta.y * size.y));
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