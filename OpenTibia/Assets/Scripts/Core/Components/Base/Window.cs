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

        protected bool _changingVisibility = false;

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
            _changingVisibility = true;
            gameObject.SetActive(true);
            _changingVisibility = false;

            if (ResetPositionOnShow)
                ResetLocalPosition();
        }


        public virtual void Hide() {
            _changingVisibility = true;
            gameObject.SetActive(false);
            _changingVisibility = false;
        }
    }
}