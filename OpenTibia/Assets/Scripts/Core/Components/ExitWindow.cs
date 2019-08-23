using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public sealed class ExitWindow : Base.Window
    {
        [SerializeField] Button _cancelButton = null;
        [SerializeField] Button _logoutButton = null;
        [SerializeField] Button _forceExitButton = null;

        protected override void Start() {
            base.Start();
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            _logoutButton.onClick.AddListener(OnLogoutButtonClicked);
            _forceExitButton.onClick.AddListener(OnForceExitButtonClicked);
        }

        void OnCancelButtonClicked() {
            UnlockFromOverlay();
            gameObject.SetActive(false);
        }

        void OnLogoutButtonClicked() {

        }

        void OnForceExitButtonClicked() {
            OpenTibiaUnity.Quiting = true;
            Application.Quit();
        }
    }
}
