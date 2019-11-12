using UnityEngine;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Components.Base
{
    [DisallowMultipleComponent]
    public class Window : Module
    {
        [System.NonSerialized] public UnityEvent onOpened = new UnityEvent();
        [System.NonSerialized] public UnityEvent onClosed = new UnityEvent();

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