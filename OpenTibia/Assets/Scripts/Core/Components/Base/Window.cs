using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Components.Base
{
    [DisallowMultipleComponent]
    public class Window : AbstractComponent
    {
        public static Window TopLockedWindow = null;
        public static List<Window> QueuedWindows = new List<Window>();

        public bool LockedToOverlay { get; private set; } = false;
        public RectTransform LockingOverlay { get; private set; } = null;

        public void LockToOverlay() {
            // is this window locked already?
            if (LockedToOverlay) {
                // is this window the current locked one?
                if (TopLockedWindow == this)
                    return;
                else if (TopLockedWindow != null)
                    QueuedWindows.Add(TopLockedWindow);
                // it's locked but must re.init
                QueuedWindows.Remove(this);
            } else if (TopLockedWindow != null) {
                // a window is locked already, push that to the stack and lock the new one.
                QueuedWindows.Add(TopLockedWindow);
            }

            // lock this window to overlay
            InternalLockToOverlay();
        }

        private void InternalLockToOverlay() {
            var gameManager = OpenTibiaUnity.GameManager;
            if (TopLockedWindow != null) {
                if (TopLockedWindow.LockingOverlay != gameManager.ActiveOverlay) {
                    TopLockedWindow.LockingOverlay.gameObject.SetActive(false);
                    gameManager.ActiveOverlay.gameObject.SetActive(true);
                }
            } else {
                gameManager.ActiveOverlay.gameObject.SetActive(true);
            }
            
            gameManager.ActiveOverlay.SetAsLastSibling();
            transform.SetParent(gameManager.ActiveCanvas.transform);
            transform.SetAsLastSibling();

            Window.TopLockedWindow = this;
            this.LockedToOverlay = true;
            this.LockingOverlay = gameManager.ActiveOverlay;
        }

        public void UnlockFromOverlay() {
            if (!LockedToOverlay)
                return;
            
            var gameManager = OpenTibiaUnity.GameManager;
            if (QueuedWindows.Count == 0) {
                // no more windows to lock, restore overlay.
                LockingOverlay.gameObject.SetActive(false);
                TopLockedWindow = null;
            } else {
                var window = QueuedWindows[0];
                QueuedWindows.RemoveAt(0);
                window.InternalLockToOverlay();
            }

            LockedToOverlay = false;
            LockingOverlay = null;
        }

        public void ResetToCenter() {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, 0, 0);
        }
    }
}