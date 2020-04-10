using UnityEngine;

namespace OpenTibiaUnity.Modules.Login
{
    public class AuthenticatorWidget : UI.Legacy.PopUpBase
    {
        // Serializable fields
        [SerializeField]
        private TMPro.TMP_InputField _token = null;

        protected override void Awake() {
            base.Awake();
            KeyMask = UI.Legacy.PopUpKeyMask.Both;

            // setup UI
            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOkButtonClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelButtonClick);
        }

        protected override void OnKeyDown(Event e, bool repeat) {
            base.OnKeyDown(e, repeat);
            switch (e.keyCode) {
                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                    e.Use();
                    OnOkButtonClick();
                    break;
                case KeyCode.Escape:
                    e.Use();
                    OnCancelButtonClick();
                    break;
            }
        }

        public override void Select() {
            base.Select();
            _token.ActivateInputField();
        }

        private void OnOkButtonClick() {
            ModulesManager.Instance.LoginWidget.LoginWithToken(_token.text);
        }

        private void OnCancelButtonClick() {
            ModulesManager.Instance.LoginWidget.Show();
        }
    }
}
