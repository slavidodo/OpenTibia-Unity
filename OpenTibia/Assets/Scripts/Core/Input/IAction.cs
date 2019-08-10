namespace OpenTibiaUnity.Core.Input
{
    internal interface IAction
    {
        bool Perform(bool repeat = false);
        IAction Clone();
    }
}
