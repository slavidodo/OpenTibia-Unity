namespace OpenTibiaUnity.Core.Input
{
    // For complicated actions such as UseActionImpl
    public interface IActionImpl
    {
        void Perform(bool repeat = false);
    }
}
