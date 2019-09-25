namespace OpenTibiaUnity.Core.Store
{
    public class StoreOpenParameters
    {
        StoreOpenParameterAction _openAction;
        OpenParameters.IStoreOpenParamater _openParamater = default;

        public StoreOpenParameterAction OpenAction { get => _openAction; }
        public OpenParameters.IStoreOpenParamater OpenParamater { get => _openParamater; }

        public StoreOpenParameters(StoreOpenParameterAction openAction, OpenParameters.IStoreOpenParamater openParam) {
            _openAction = openAction;
            _openParamater = openParam;
        }

        public void WriteTo(Communication.Internal.CommunicationStream message) {
            message.WriteEnum(_openAction);
            if (_openParamater != null)
                _openParamater.WriteTo(message);

            // unknown bytes (but are likely to be enums)
            message.WriteUnsignedByte(0);
            message.WriteUnsignedByte(0);
        }
    }
}
