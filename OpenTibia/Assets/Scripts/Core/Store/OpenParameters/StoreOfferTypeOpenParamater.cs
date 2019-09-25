using OpenTibiaUnity.Core.Communication.Internal;

namespace OpenTibiaUnity.Core.Store.OpenParameters
{
    public class StoreOfferTypeOpenParamater : IStoreOpenParamater
    {
        StoreOfferType _type;

        public StoreOfferType Type { get => _type; }

        public StoreOfferTypeOpenParamater(StoreOfferType addition) {
            _type = addition;
        }

        public void WriteTo(CommunicationStream stream) {
            stream.WriteEnum(_type);
        }
    }
}
