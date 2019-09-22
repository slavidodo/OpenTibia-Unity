using System;

namespace OpenTibiaUnity.Assets.Scripts.Core.Communication.Attributes
{
    class ClientVersionAttribute : Attribute
    {
        private int _clientVersion;

        public ClientVersionAttribute(int clientVersion) {
            _clientVersion = clientVersion;
        }

        public int ClientVersion { get => _clientVersion; }
    }
}
