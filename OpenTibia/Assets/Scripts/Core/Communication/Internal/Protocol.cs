using System;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public abstract class Protocol
    {
        protected Connection _connection;
        
        protected NetworkPacketWriter _packetWriter;
        protected CommunicationStream _inputStream;
        protected NetworkPacketReader _packetReader;
        protected Cryptography.XTEA _xTEA;


        public Protocol() {
            CreateNewInputBuffer();

            _packetWriter = new NetworkPacketWriter();
            _xTEA = new Cryptography.XTEA();
        }

        private void CreateNewInputBuffer() {
            _inputStream = new CommunicationStream();
            _packetReader = new NetworkPacketReader(_inputStream);
        }

        public virtual void Connect(string address, int port) {
            if (_connection != null)
                throw new InvalidOperationException("Protocol.Connect: Trying to establish a connection before terminating.");

            _packetWriter.onPacketFinished.AddListener(OnPacketWriterFinished);
            _packetReader.onPacketReady.AddListener(OnPacketReaderReady);

            _connection = new Connection();
            AddConnectionListeners();
            
            _connection.Connect(address, port);
        }

        public virtual void Disconnect(bool dispatch = true) {
            if (!!_connection) {
                _packetWriter.onPacketFinished.RemoveListener(OnPacketWriterFinished);
                _packetReader.onPacketReady.RemoveListener(OnPacketReaderReady);

                // there is an public wrapper for disposing the used socket
                // there is no need to reuse the socket
                RemoveConnectionListeners();
                _connection.Disconnect();
                _connection = null;
            }
        }

        protected void AddConnectionListeners() {
            if (!!_connection) {
                _connection.onConnectionEstablished.AddListener(OnConnectionEstablished);
                _connection.onConnectionTerminated.AddListener(OnConnectionTerminated);
                _connection.onConnectionError.AddListener(OnConnectionError);
                _connection.onConnectionSocketError.AddListener(OnConnectionSocketError);
                _connection.onConnectionReceived.AddListener(OnConnectionReceived);
                _connection.onConnectionSent.AddListener(OnConnectionSent);
            }
        }

        protected void RemoveConnectionListeners() {
            if (!!_connection) {
                _connection.onConnectionEstablished.RemoveListener(OnConnectionEstablished);
                _connection.onConnectionTerminated.RemoveListener(OnConnectionTerminated);
                _connection.onConnectionError.RemoveListener(OnConnectionError);
                _connection.onConnectionSocketError.RemoveListener(OnConnectionSocketError);
                _connection.onConnectionReceived.RemoveListener(OnConnectionReceived);
                _connection.onConnectionSent.RemoveListener(OnConnectionSent);
            }
        }

        protected virtual void OnConnectionEstablished() { }
        protected virtual void OnConnectionTerminated() { }
        protected virtual void OnConnectionError(string error, bool disconnecting = false) { }
        protected virtual void OnConnectionSocketError(System.Net.Sockets.SocketError e, string error) { }
        protected virtual void OnConnectionReceived(CommunicationStream stream) {
            try {
                if (!_connection || !_connection.Established || _connection.Terminated)
                    return;
                
                _inputStream.SetLength(0);
                _inputStream.Position = Connection.PacketLengthPos;
                stream.CopyTo(_inputStream);
                _inputStream.Position = 0;

                _packetReader.PreparePacket();
            } catch (Exception e) {
                OnConnectionError($"Protocol.OnConnectionReceived: Failed to prepare packet ({e.Message}).");
            }
        }
        protected virtual void OnConnectionSent(CommunicationStream stream) { }

        protected virtual void OnPacketReaderReady() {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                if (!!_connection && _connection.Established) {
                    OnCommunicationDataReady();
                    
                    // TODO; this is incorrect as it has to wait
                    // for the main thread work to be done.
                    if (!!_connection && _connection.Established && !_connection.Terminated)
                        _connection.Receive();
                }
            });
        }

        protected virtual void OnCommunicationDataReady() {}

        protected virtual void OnPacketWriterFinished() {
            if (!!_connection && _connection.Established)
                _connection.Send(_packetWriter.OutputPacketBuffer);
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
