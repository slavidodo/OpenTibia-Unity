namespace OpenTibiaUnity.Core.Input
{
    public interface IAction
    {
        bool Perform(bool repeat = false);
        IAction Clone();
    }
}
