namespace OpenTibiaUnity.Core.InputManagment
{
    // For complicated actions such as UseActionImpl
    public interface IActionImpl
    {
        void Perform(bool repeat = false);
    }
}
