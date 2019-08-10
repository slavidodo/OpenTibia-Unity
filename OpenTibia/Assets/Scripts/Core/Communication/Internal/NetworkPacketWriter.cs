using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    internal sealed class NetworkPacketWriter
    {
        internal class PacketFinishedEvent : UnityEvent { }

        private Cryptography.XTEA m_XTEA = null;
        private ByteArray m_MessageBuffer = new ByteArray();
        private ByteArray m_OutputBuffer = new ByteArray();
        private uint m_SequenceNumber = 0;

        internal PacketFinishedEvent onPacketFinished = new PacketFinishedEvent();

        internal Cryptography.XTEA XTEA {
            get => m_XTEA;
            set => m_XTEA = value;
        }

        internal ByteArray OutputPacketBuffer {
            get => m_OutputBuffer;
        }

        internal ByteArray CreateMessage() {
            // separate the body from the whole message
            // to make it easier to perform actions on the body

            m_MessageBuffer.Length = 0;
            m_MessageBuffer.Position = 0;
            m_OutputBuffer.Length = 0;
            m_OutputBuffer.Position = 0;
            return m_MessageBuffer;
        }

        internal void FinishMessage() {
            m_OutputBuffer.Length = 0;
            m_OutputBuffer.Position = 0;

            int pos = Connection.PacketLengthPos + Connection.PacketLengthSize;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber))
                pos += Connection.SequenceNumberSize;
            else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum))
                pos += Connection.ChecksumSize;
            
            m_OutputBuffer.Position = pos;
            int messageSize = m_MessageBuffer.Position;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPacketEncryption) && m_XTEA != null) {
                m_OutputBuffer.WriteUnsignedShort((ushort)messageSize);
                m_OutputBuffer.WriteBytes(m_MessageBuffer, 0, messageSize);
                m_XTEA.Encrypt(m_OutputBuffer, pos, m_OutputBuffer.Length - pos);
            } else {
                m_OutputBuffer.WriteBytes(m_MessageBuffer, 0, messageSize);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber)) {
                m_OutputBuffer.Position = Connection.SequenceNumberPos;
                m_OutputBuffer.WriteUnsignedInt(m_SequenceNumber++);
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum)) {
                uint checksum = Cryptography.Adler32Checksum.CalculateAdler32Checksum(m_OutputBuffer, pos, m_OutputBuffer.Length - pos);
                m_OutputBuffer.Position = Connection.ChecksumPos;
                m_OutputBuffer.WriteUnsignedInt(checksum);
            }
            
            m_OutputBuffer.Position = Connection.PacketLengthPos;
            m_OutputBuffer.WriteShort((short)(m_OutputBuffer.Length - Connection.PacketLengthSize));
            m_OutputBuffer.Position = 0;

            onPacketFinished.Invoke();
        }
    }
}
