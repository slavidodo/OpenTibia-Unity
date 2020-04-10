using UnityEngine;

namespace OpenTibiaUnity.Modules.ModalDialog
{
    using ProtocolDialog = Core.Communication.Game.ProtocolModalDialog;
    using ProtocolEntity = Core.Communication.Game.ProtocolModalDialogEntity;

    public class ServerModalDialog : UI.Legacy.MessageWidgetBase
    {
        // serialized fields
        [SerializeField]
        private UI.Legacy.ScrollRect _choiceScrollRect = null;

        // non-serialized fields
        [System.NonSerialized]
        public byte DefaultEnterButton = 0;
        [System.NonSerialized]
        public byte DefaultEscapeButton = 0;

        private ProtocolEntity[] _choices = null;
        public ProtocolEntity[] choices {
            get => _choices;
            set {
                _choices = value;
                if (_choices != null && _choices.Length > 0) {
                    // enable choices
                    _choiceScrollRect.gameObject.SetActive(true);

                    // destroy choices
                    foreach (Transform child in _choiceScrollRect.content)
                        Destroy(child.gameObject);

                    // add choices
                    foreach (var choice in _choices)
                        AddChoice(choice);
                } else {
                    _choiceScrollRect.gameObject.SetActive(false);
                }
            }
        }

        private ProtocolEntity[] _buttons = null;
        public ProtocolEntity[] buttons {
            get => _buttons;
            set {
                _buttons = value;
                if (_buttons != null && _buttons.Length > 0) {
                    // destroy buttons
                    foreach (Transform child in _footer.transform)
                        Destroy(child.gameObject);

                    // add buttons
                    foreach (var button in _buttons)
                        AddButton(button);
                }
            }
        }

        protected override void OnKeyDown(Event e, bool repeat) {
            base.OnKeyDown(e, repeat);

            switch (e.keyCode) {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    ConfirmModalDialog(DefaultEnterButton);
                    break;

                case KeyCode.Escape:
                    ConfirmModalDialog(DefaultEscapeButton);
                    break;
            }
        }

        public void AddChoice(ProtocolEntity entity) {
            var label = Instantiate(OpenTibiaUnity.GameManager.DefaultLabel, _choiceScrollRect.content);
            label.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            label.text = entity.Text;
            label.fontSize = 9;
        }

        public void AddButton(ProtocolEntity entity) {
            AddButton(entity.Text, () => ConfirmModalDialog(entity.Id));
        }

        private void ConfirmModalDialog(byte buttonId) {
            Debug.Log("Confirming: " + buttonId);
        }

        public static ServerModalDialog CreateModalDialog(ProtocolDialog pModalDialog) {
            var modalDialog = Instantiate(ModulesManager.Instance.ServerModalDialogPrefab, OpenTibiaUnity.GameManager.ActiveCanvas.transform);
            modalDialog.gameObject.SetActive(false);
            modalDialog.title = pModalDialog.Title;
            modalDialog.message = pModalDialog.Message;
            modalDialog.buttons = pModalDialog.Buttons;
            modalDialog.choices = pModalDialog.Choices;
            modalDialog.DefaultEnterButton = pModalDialog.DefaultEnterButton;
            modalDialog.DefaultEscapeButton = pModalDialog.DefaultEscapeButton;
            modalDialog.Priority = PopupPriorityDefault + (pModalDialog.Priority ? 1 : 0);
            modalDialog.KeyMask = UI.Legacy.PopUpKeyMask.Both;
            modalDialog.Show();
            return modalDialog;
        }
    }
}
