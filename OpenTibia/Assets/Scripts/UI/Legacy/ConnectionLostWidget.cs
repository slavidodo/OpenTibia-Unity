using UnityEngine;

namespace OpenTibiaUnity.UI.Legacy
{
    public class ConnectionLostWidget : TimeoutWaitWidget
    {
        protected override void Awake() {
            base.Awake();
            message = TextResources.CONNECTION_LOST_MESSAGE;
            KeyMask = PopUpKeyMask.Escape;

            AddButton(PopUpButtonMask.Abort, OnAbort);
        }

        protected override void OnKeyDown(Event e, bool repeat) {
            base.OnKeyDown(e, repeat);
            switch (e.keyCode) {
                case KeyCode.Escape:
                    e.Use();
                    OnAbort();
                    break;
            }
        }

        private void OnAbort() {
            var protocolGame = OpenTibiaUnity.GameManager.ProtocolGame;
            if (!!protocolGame)
                protocolGame.Disconnect();
        }

        protected override void UpdateMessage() {
            _message.text = string.Format(TextResources.CONNECTION_LOST_TEXT, message, GetTimeString(RemainingTime));
            _message.ForceMeshUpdate(true);
            RecalculateMessageBounds();
        }

        public static ConnectionLostWidget CreateWidget() {
            var widget = Instantiate(OpenTibiaUnity.GameManager.ConnectionLostWidgetPrefab, OpenTibiaUnity.GameManager.ActiveCanvas.transform);
            widget.TimeOut = 60 * 1000;

            return widget;
        }
    }
}
