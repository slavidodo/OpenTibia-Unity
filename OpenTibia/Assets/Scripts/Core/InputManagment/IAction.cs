namespace OpenTibiaUnity.Core.InputManagment
{
    public interface IAction
    {
        bool Perform(bool repeat = false);
        IAction Clone();
    }
}
