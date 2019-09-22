using OpenTibiaUnity.Core.Communication.Internal;

namespace OpenTibiaUnity.Core.Store.OpenParameters
{
    public class StoreCategoryTypeOpenParamater : IStoreOpenParamater
    {
        StoreCategoryType _type;

        public StoreCategoryType Type { get => _type; }

        public StoreCategoryTypeOpenParamater(StoreCategoryType type) {
            _type = type;
        }

        public void WriteToMessage(ByteArray message) {
            message.WriteEnum(_type);
        }
    }
}
