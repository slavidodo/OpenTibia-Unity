using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    internal sealed class ExitWindow : Base.Window
    {
        [SerializeField] Button m_CancelButton = null;
        [SerializeField] Button m_LogoutButton = null;
        [SerializeField] Button m_ForceExitButton = null;

        protected override void Start() {
            base.Start();
            m_CancelButton.onClick.AddListener(OnCancelButtonClicked);
            m_LogoutButton.onClick.AddListener(OnLogoutButtonClicked);
            m_ForceExitButton.onClick.AddListener(OnForceExitButtonClicked);
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
