using System;

namespace OpenTibiaUnity.Core.Communication.Attributes
{
    class ClientVersionAttribute : Attribute
    {
        private int _clientVersion;
        public int ClientVersion { get => _clientVersion; }

        public ClientVersionAttribute(int clientVersion) {
            _clientVersion = clientVersion;
        }
    }
}
