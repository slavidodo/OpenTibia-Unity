using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public sealed class ExitWindow : Base.Window
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] Button m_CancelButton;
        [SerializeField] Button m_LogoutButton;
        [SerializeField] Button m_ForceExitButton;
#pragma warning restore CS0649 // never assigned to

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
