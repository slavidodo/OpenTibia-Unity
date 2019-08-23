using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public class NetworkPacketReader
    {
        public class PacketReadyEvent : UnityEvent { }

        private Cryptography.XTEA _xTEA = null;
        private ByteArray _inputBuffer = null;

        private bool _compressed = false;

        public PacketReadyEvent onPacketReady = new PacketReadyEvent();

        public Cryptography.XTEA XTEA {
            get => _xTEA;
            set => _xTEA = value;
        }

        public bool Compressed {
            get => _compressed;
        }

        public NetworkPacketReader(ByteArray input) {
            _inputBuffer = input;
        }

        public bool BytesAvailable(int bytes) {
            return _inputBuffer.BytesAvailable >= bytes;
        }

        public bool PreparePacket() {
            int payloadOffset = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber)) {
                uint recvCompression = _inputBuffer.ReadUnsignedInt();
                payloadOffset = _inputBuffer.Position;
                
                _compressed = (recvCompression & 1U << 31) != 0;
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum)) {
                var recvChecksum = _inputBuffer.ReadUnsignedInt();
                payloadOffset = _inputBuffer.Position;
                uint checksum = Cryptography.Adler32Checksum.CalculateAdler32Checksum(_inputBuffer, payloadOffset, _inputBuffer.Length - payloadOffset);
                if (recvChecksum != checksum)
                    return false;

                _inputBuffer.Position = payloadOffset;
                _compressed = false;
            }

            if (_xTEA != null) {
                int length = _inputBuffer.Length - payloadOffset;
                if (_xTEA.Decrypt(_inputBuffer, payloadOffset, length) == 0)
                    return false;
            }
            
            onPacketReady.Invoke();
            return true;
        }
    }
}
