namespace OpenTibiaUnity.Core.Components.Base
{
    public class Module : AbstractComponent
    {
        protected override void Awake() {
            base.Awake();

            var gameManager = OpenTibiaUnity.GameManager;
            if (gameManager != null) {
                // setup module
                gameManager.RegisterModule(this);

                // setup version-change control
                gameManager.onClientVersionChange.AddListener(OnClientVersionChange);
                if (gameManager.ClientVersion != 0)
                    OnClientVersionChange(0, gameManager.ClientVersion);
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            var gameManager = OpenTibiaUnity.GameManager;
            if (gameManager != null) {
                gameManager.onClientVersionChange.RemoveListener(OnClientVersionChange);
                gameManager.UnregisterModule(this);
            }
        }

        protected virtual void OnClientVersionChange(int oldVersion, int newVersion) { }
    }
}
