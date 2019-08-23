using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    public class AuthenticatorWindow : Core.Components.Base.Window
    {
        [SerializeField] private TMPro.TMP_InputField _tokenInput = null;
        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;

        protected override void Start() {
            base.Start();

            _oKButton.onClick.AddListener(OnOkClick);
            _cancelButton.onClick.AddListener(OnCancelClick);
        }

        private void OnOkClick() {
            Close();
            ModulesManager.Instance.LoginWindow.DoLoginWithNewToken(_tokenInput.text);
        }

        private void OnCancelClick() {
            Close();
            ModulesManager.Instance.LoginWindow.Open();
        }
    }
}
