namespace OpenTibiaUnity.UI.Legacy
{
    public interface IBasicUIComponent
    {
        bool IsEnabled();
        void SetEnabled(bool enabled);

        void Enable();
        void Disable();
    }
}
