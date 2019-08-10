namespace OpenTibiaUnity.Core.Input
{
    // For complicated actions such as UseActionImpl
    internal interface IActionImpl
    {
        void Perform(bool repeat = false);
    }
}
