namespace OpenTibiaUnity.Core.Store.OpenParameters
{
    public interface IStoreOpenParamater
    {
        void WriteTo(Communication.Internal.CommunicationStream message);
    }
}
