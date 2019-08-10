namespace OpenTibiaUnity.Core.Components.Base
{
    internal class Module : AbstractComponent
    {
        protected virtual new void Awake() {
            base.Awake();
            if (OpenTibiaUnity.GameManager != null)
                OpenTibiaUnity.GameManager.RegisterModule(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (OpenTibiaUnity.GameManager != null)
                OpenTibiaUnity.GameManager.UnregisterModule(this);
        }
    }
}
