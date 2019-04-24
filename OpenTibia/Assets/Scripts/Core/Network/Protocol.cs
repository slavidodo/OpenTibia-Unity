using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Network
{
    public abstract class Protocol
    {
        protected Connection m_Connection;

        protected uint[] m_XteaKey;
        
        public UnityEvent onDisconnect = new UnityEvent();

        public bool IsConnected {
            get {
                return m_Connection != null && m_Connection.IsConnected;
            }
        }

        public bool ChecksumEnabled { get; set; } = false;
        public bool XteaEnabled { get; set; } = false;

        private uint m_SequenceNumber = 0;

        public abstract void Connect();

        public void Connect(string ip, int port) {
            m_Connection = new Connection(OnConnect, OnDisconnect, OnError, OnRecv);
            m_Connection.Connect(ip, port);
        }

        public virtual void Disconnect() {
            m_Connection?.Disconnect();
            m_Connection = null;
        }

        public void BeginRecv() {
            m_Connection?.BeginRecv();
        }

        public void WriteToOutput(OutputMessage message, bool raw = false) {
            if (!raw) {
                if (XteaEnabled)
                    XteaEncrypt(message);

                if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameProtocolChecksum))
                    AddChecksum(message);
                else if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameProtocolSequenceNumber))
                    AddSequenceNumber(message);

                AddMessageLength(message);
            }

            m_Connection?.Send(message);
        }

        void AddChecksum(OutputMessage message) {
            if (ChecksumEnabled) {
                uint checksum = Adler32(message.GetBuffer().ToArray(), (uint)message.GetBufferLength());

                int pos = message.Tell();
                message.Seek(0);
                message.AddU32(checksum);
                message.IncreaseHeaderLength(sizeof(uint));
                message.Seek(pos + sizeof(uint));
            }
        }
        
        void AddSequenceNumber(OutputMessage message) {
            int pos = message.Tell();
            message.Seek(0);
            message.AddU32(m_SequenceNumber++);
            message.IncreaseHeaderLength(sizeof(uint));
            message.Seek(pos + sizeof(uint));
        }

        void AddMessageLength(OutputMessage message) {
            int pos = message.Tell();
            message.Seek(0);
            message.AddU16((ushort)message.GetBufferLength());
            message.IncreaseHeaderLength(sizeof(ushort));
            message.Seek(pos + sizeof(ushort));
        }
        
        public void GenerateXteaKey(System.Random random = null) {
            m_XteaKey = new uint[4];
            if (random == null) random = new System.Random();
            m_XteaKey[0] = (uint)random.Next(0xFFFFFFF);
            m_XteaKey[1] = (uint)random.Next(0xFFFFFFF);
            m_XteaKey[2] = (uint)random.Next(0xFFFFFFF);
            m_XteaKey[3] = (uint)random.Next(0xFFFFFFF);
        }

        public void AddXteaKey(OutputMessage message) {
            message.AddU32(m_XteaKey[0]);
            message.AddU32(m_XteaKey[1]);
            message.AddU32(m_XteaKey[2]);
            message.AddU32(m_XteaKey[3]);
        }

        protected virtual void OnConnect() { }
        protected virtual void OnDisconnect() { onDisconnect.Invoke(); }
        protected virtual void OnError(SocketError _, string __) { }
        protected virtual void OnRecv(InputMessage message) {
            if (ChecksumEnabled && !message.ReadChecksum()) {
                throw new Exception("Unable to read checksum.");
            }
            
            //CheckCompression(message);
            if (XteaEnabled) {
                try {
                    XteaDecrypt(message);
                } catch (Exception e) {
                    throw new Exception("Protocol.OnRecv: Failed to decrypt the message.\n" + e);
                }
            }
        }

        void DecryptMessage(InputMessage message) {
            uint sequence = message.GetU32();
            if ((sequence & 1 << 31) != 0) {
                // TODO[priority=med]: inflate the message (maybe using IO.Compression)
            }
        }

        private void XteaEncrypt(OutputMessage message) {
            AddMessageLength(message);
            
            int encryptedSize = message.GetBufferLength();
            if ((encryptedSize % 8) != 0) {
                int n = 8 - (encryptedSize % 8);
                message.AddPaddingBytes(n);
                encryptedSize += n;
            }

            int indexedSize = message.GetBufferLength();
            byte[] buffer = message.GetIndexedBuffer(indexedSize);
            
            int readPos = 0;
            while (readPos < encryptedSize / 4) {
                uint v0 = BitConverter.ToUInt32(buffer, readPos * 4);
                uint v1 = BitConverter.ToUInt32(buffer, (readPos + 1) * 4);
                uint delta = 0x61C88647;
                uint sum = 0;

                for (int i = 0; i < 32; i++) {
                    v0 += ((v1 << 4 ^ v1 >> 5) + v1) ^ (sum + m_XteaKey[sum & 3]);
                    sum -= delta;
                    v1 += ((v0 << 4 ^ v0 >> 5) + v0) ^ (sum + m_XteaKey[sum >> 11 & 3]);
                }

                int tmpReadPos = 0;

                byte[] v0Array = BitConverter.GetBytes(v0);
                byte[] v1Array = BitConverter.GetBytes(v1);
                foreach (byte v in v0Array)
                    buffer[readPos * 4 + tmpReadPos++] = v;
                foreach (byte v in v1Array)
                    buffer[readPos * 4 + tmpReadPos++] = v;

                readPos += 2;
            }
            
            message.ReplaceIndexedBuffer(buffer);
        }

        void XteaDecrypt(InputMessage message) {
            int encryptedSize = message.GetUnreadSize();
            if (encryptedSize % 8 != 0)
                throw new Exception("invalid encrypted network message with size: " + (encryptedSize % 8) + ", " + message.GetU16());

            byte[] unreadBuffer = message.GetUnreadBuffer();

            int readPos = 0;
            while (readPos < encryptedSize / 4) {
                uint v0 = BitConverter.ToUInt32(unreadBuffer, readPos * 4);
                uint v1 = BitConverter.ToUInt32(unreadBuffer, (readPos + 1) * 4);
                uint delta = 0x61C88647;
                uint sum = 0xC6EF3720;

                for (int i = 0; i < 32; i++) {
                    v1 -= ((v0 << 4 ^ v0 >> 5) + v0) ^ (sum + m_XteaKey[sum >> 11 & 3]);
                    sum += delta;
                    v0 -= ((v1 << 4 ^ v1 >> 5) + v1) ^ (sum + m_XteaKey[sum & 3]);
                }
                
                int tmpReadPos = 0;

                byte[] v0Array = BitConverter.GetBytes(v0);
                byte[] v1Array = BitConverter.GetBytes(v1);
                foreach (byte v in v0Array)
                    unreadBuffer[readPos * 4 + tmpReadPos++] = v;
                foreach (byte v in v1Array)
                    unreadBuffer[readPos * 4 + tmpReadPos++] = v;

                readPos = readPos + 2;
            }

            ushort decryptedSize = (ushort)(BitConverter.ToUInt16(unreadBuffer, 0) + 2);
            int sizeDelta = decryptedSize - encryptedSize;
            if (sizeDelta > 0 || -sizeDelta > encryptedSize)
                throw new Exception("invalid decrypted network message with sizeDelta: " + sizeDelta);

            byte[] newBuffer = new byte[decryptedSize - 2];
            Array.Copy(unreadBuffer, 2, newBuffer, 0, decryptedSize - 2);

            message.SetBuffer(newBuffer);
        }

        public static uint Adler32(byte[] buffer, uint size) {
            uint a = 1, b = 0, tlen;
            int i = 0;
            while (size > 0) {
                tlen = size > 5552 ? 5552 : size;
                size -= tlen;
                do {
                    a += buffer[i++];
                    b += a;
                } while (--tlen != 0);

                a %= 65521;
                b %= 65521;
            }
            return (b << 16) | a;
        }

        public static bool operator !(Protocol protocol) {
            return protocol == null;
        }

        public static bool operator true(Protocol protocol) {
            return !(!protocol);
        }

        public static bool operator false(Protocol protocol) {
            return !(protocol);
        }
    }
}