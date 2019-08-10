using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Components.Base
{
    [DisallowMultipleComponent]
    internal class Window : Module
    {
        internal UnityEvent onOpened = new UnityEvent();
        internal UnityEvent onClosed = new UnityEvent();

        [SerializeField] internal bool ResetPositionOnShow = true;
        
        internal bool Visible { get => enabled && gameObject.activeSelf; }

        internal void OpenWindow(bool resetPosition = true) {
            ShowWindow();
            LockToOverlay();
            Select();
            
            onOpened.Invoke();
        }

        internal void CloseWindow() {
            HideWindow();
            UnlockFromOverlay();
            onClosed.Invoke();
        }

        internal virtual void ShowWindow() {
            gameObject.SetActive(true);

            if (ResetPositionOnShow)
                ResetLocalPosition();
        }


        internal virtual void HideWindow() {
            gameObject.SetActive(false);
        }

        internal void Select() {
            if (EventSystem.current.alreadySelecting)
                return;

            OpenTibiaUnity.EventSystem.SetSelectedGameObject(gameObject);
        }
    }
}