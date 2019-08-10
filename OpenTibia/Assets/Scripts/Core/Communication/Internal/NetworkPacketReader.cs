using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    internal class NetworkPacketReader
    {
        internal class PacketReadyEvent : UnityEvent { }

        private Cryptography.XTEA m_XTEA = null;
        private ByteArray m_InputBuffer = null;

        private bool m_Compressed = false;

        internal PacketReadyEvent onPacketReady = new PacketReadyEvent();

        internal Cryptography.XTEA XTEA {
            get => m_XTEA;
            set => m_XTEA = value;
        }

        internal bool Compressed {
            get => m_Compressed;
        }

        internal NetworkPacketReader(ByteArray input) {
            m_InputBuffer = input;
        }

        internal bool BytesAvailable(int bytes) {
            return m_InputBuffer.BytesAvailable >= bytes;
        }

        internal bool PreparePacket() {
            int payloadOffset = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber)) {
                uint recvCompression = m_InputBuffer.ReadUnsignedInt();
                payloadOffset = m_InputBuffer.Position;
                
                m_Compressed = (recvCompression & 1U << 31) != 0;
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum)) {
                var recvChecksum = m_InputBuffer.ReadUnsignedInt();
                payloadOffset = m_InputBuffer.Position;
                uint checksum = Cryptography.Adler32Checksum.CalculateAdler32Checksum(m_InputBuffer, payloadOffset, m_InputBuffer.Length - payloadOffset);
                if (recvChecksum != checksum)
                    return false;

                m_InputBuffer.Position = payloadOffset;
                m_Compressed = false;
            }

            if (m_XTEA != null) {
                int length = m_InputBuffer.Length - payloadOffset;
                if (m_XTEA.Decrypt(m_InputBuffer, payloadOffset, length) == 0)
                    return false;
            }
            
            onPacketReady.Invoke();
            return true;
        }
    }
}
