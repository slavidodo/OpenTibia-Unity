using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Components.Base
{
    [DisallowMultipleComponent]
    public class Window : Module
    {
        public UnityEvent onOpened = new UnityEvent();
        public UnityEvent onClosed = new UnityEvent();

        public bool ResetPositionOnShow = true;
        
        public bool Visible { get => enabled && gameObject.activeSelf; }

        public void Open(bool resetPosition = true) {
            Show();
            LockToOverlay();
            Select();
            
            onOpened.Invoke();
        }

        public override void Close() {
            Hide();
            UnlockFromOverlay();
            onClosed.Invoke();
        }

        public virtual void Show() {
            gameObject.SetActive(true);

            if (ResetPositionOnShow)
                ResetLocalPosition();
        }


        public virtual void Hide() {
            gameObject.SetActive(false);
        }
    }
}