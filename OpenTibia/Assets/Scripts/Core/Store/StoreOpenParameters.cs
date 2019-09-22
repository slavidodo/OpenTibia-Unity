namespace OpenTibiaUnity.Core.Store
{
    public class StoreOpenParameters
    {
        StoreOpenParameterAction _openAction;
        OpenParameters.IStoreOpenParamater _openParamater = default;

        public StoreOpenParameters(StoreOpenParameterAction openAction, OpenParameters.IStoreOpenParamater openParam) {
            _openAction = openAction;
            _openParamater = openParam;
        }

        public void WriteTo(Communication.Internal.ByteArray message) {
            message.WriteEnum(_openAction);
            _openParamater.WriteToMessage(message);

            // unknown bytes (but are likely to be enums)
            message.WriteUnsignedByte(0);
            message.WriteUnsignedByte(0);
        }
    }
}
