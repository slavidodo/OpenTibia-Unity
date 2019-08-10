using System;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    internal abstract class Protocol
    {
        protected Connection m_Connection;
        
        protected NetworkPacketWriter m_PacketWriter;
        protected ByteArray m_InputBuffer;
        protected NetworkPacketReader m_PacketReader;
        protected Cryptography.XTEA m_XTEA;


        internal Protocol() {
            CreateNewInputBuffer();

            m_PacketWriter = new NetworkPacketWriter();
            m_XTEA = new Cryptography.XTEA();
        }

        private void CreateNewInputBuffer() {
            m_InputBuffer = new ByteArray();
            m_PacketReader = new NetworkPacketReader(m_InputBuffer);
        }

        internal virtual void Connect(string address, int port) {
            if (m_Connection != null)
                throw new InvalidOperationException("Protocol.Connect: Trying to establish a connection before terminating.");

            m_PacketWriter.onPacketFinished.AddListener(OnPacketWriterFinished);
            m_PacketReader.onPacketReady.AddListener(OnPacketReaderReady);


            m_Connection = new Connection();
            AddConnectionListeners();
            
            m_Connection.Connect(address, port);
        }

        internal virtual void Disconnect(bool dispatch = true) {
            if (!!m_Connection) {
                m_PacketWriter.onPacketFinished.RemoveListener(OnPacketWriterFinished);
                m_PacketReader.onPacketReady.RemoveListener(OnPacketReaderReady);

                // there is an internal wrapper for disposing the used socket
                // there is no need to reuse the socket
                RemoveConnectionListeners();
                m_Connection.Disconnect();
                m_Connection = null;
            }
        }

        protected void AddConnectionListeners() {
            if (!!m_Connection) {
                m_Connection.onConnectionEstablished.AddListener(OnConnectionEstablished);
                m_Connection.onConnectionTerminated.AddListener(OnConnectionTerminated);
                m_Connection.onConnectionError.AddListener(OnConnectionError);
                m_Connection.onConnectionSocketError.AddListener(OnConnectionSocketError);
                m_Connection.onConnectionReceived.AddListener(OnConnectionReceived);
                m_Connection.onConnectionSent.AddListener(OnConnectionSent);
            }
        }

        protected void RemoveConnectionListeners() {
            if (!!m_Connection) {
                m_Connection.onConnectionEstablished.RemoveListener(OnConnectionEstablished);
                m_Connection.onConnectionTerminated.RemoveListener(OnConnectionTerminated);
                m_Connection.onConnectionError.RemoveListener(OnConnectionError);
                m_Connection.onConnectionSocketError.RemoveListener(OnConnectionSocketError);
                m_Connection.onConnectionReceived.RemoveListener(OnConnectionReceived);
                m_Connection.onConnectionSent.RemoveListener(OnConnectionSent);
            }
        }

        protected virtual void OnConnectionEstablished() { }
        protected virtual void OnConnectionTerminated() { }
        protected virtual void OnConnectionError(string error, bool disconnecting = false) { }
        protected virtual void OnConnectionSocketError(System.Net.Sockets.SocketError e, string error) { }
        protected virtual void OnConnectionReceived(ByteArray message) {
            try {
                if (!m_Connection || !m_Connection.Established || m_Connection.Terminated)
                    return;

                m_InputBuffer.Position = Connection.PacketLengthPos;
                m_InputBuffer.Length = 0;
                m_InputBuffer.WriteBytes(message.Buffer, 0, message.Length);
                m_InputBuffer.Position = 0;

                if (!m_PacketReader.PreparePacket())
                    OnConnectionError("Protocol.OnConnectionReceived: Failed to prepare packet.");
            } catch (Exception) {
                OnConnectionError("Protocol.OnConnectionReceived: Failed to prepare packet.");
            }
        }
        protected virtual void OnConnectionSent(ByteArray message) { }

        protected virtual void OnPacketReaderReady() {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                if (!!m_Connection && m_Connection.Established) {
                    OnCommunicationDataReady();
                    
                    if (!!m_Connection && m_Connection.Established && !m_Connection.Terminated)
                        m_Connection.Receive();
                }
            });
        }

        protected virtual void OnCommunicationDataReady() {}

        protected virtual void OnPacketWriterFinished() {
            if (!!m_Connection && m_Connection.Established)
                m_Connection.Send(m_PacketWriter.OutputPacketBuffer);
        }

        public static bool operator !(Protocol instance) {
            return instance == null;
        }

        public static bool operator true(Protocol instance) {
            return !!instance;
        }

        public static bool operator false(Protocol instance) {
            return !instance;
        }
    }
}
