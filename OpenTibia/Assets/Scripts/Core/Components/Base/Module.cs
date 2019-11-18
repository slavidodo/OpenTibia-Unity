namespace OpenTibiaUnity.Core.Components.Base
{
    public class Module : AbstractComponent
    {
        protected bool _destoryable = false;
        protected bool _closable = false;

        public bool Destroyable { get => _destoryable; }
        public bool Closable { get => _closable; }

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

        public virtual void CloseWithoutNotify() {

        }

        public virtual void Close() {

        }
    }
}
