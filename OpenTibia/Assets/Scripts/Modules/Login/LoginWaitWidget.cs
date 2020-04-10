namespace OpenTibiaUnity.Modules.Login
{
    public class LoginWaitWidget : UI.Legacy.TimeoutWaitWidget
    {
        protected override void UpdateMessage() {
            _message.text = string.Format(TextResources.LOGIN_WAIT_TEXT, message, GetTimeString(RemainingTime));
            _message.ForceMeshUpdate(true);
            RecalculateMessageBounds();
        }
    }
}
