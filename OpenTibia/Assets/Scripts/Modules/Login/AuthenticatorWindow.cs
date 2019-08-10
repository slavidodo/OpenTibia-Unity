using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    internal class AuthenticatorWindow : Core.Components.Base.Window
    {
        [SerializeField] private TMPro.TMP_InputField m_TokenInput = null;
        [SerializeField] private Button m_OKButton = null;
        [SerializeField] private Button m_CancelButton = null;

        protected override void Start() {
            base.Start();

            m_OKButton.onClick.AddListener(OnOkClick);
            m_CancelButton.onClick.AddListener(OnCancelClick);
        }

        private void OnOkClick() {
            CloseWindow();
            ModulesManager.Instance.LoginWindow.DoLoginWithNewToken(m_TokenInput.text);
        }

        private void OnCancelClick() {
            CloseWindow();
            ModulesManager.Instance.LoginWindow.OpenWindow();
        }
    }
}
