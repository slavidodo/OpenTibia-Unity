using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public class AsyncStateHolder
    {
        public byte[] AsyncBuffer;
        public int State = 0;

        public int Required {
            get => AsyncBuffer.Length - State;
        }

        public bool Finished {
            get => State == AsyncBuffer.Length;
        }

        public byte[] Buffer {
            get => AsyncBuffer;
        }

        public AsyncStateHolder(byte[] buffer, int state = 0) {
            AsyncBuffer = buffer;
            State = state;
        }
    }

    public class Connection
    {
        public const int HeaderPos = 0;

        public const int PacketLengthPos = 0;
        public const int PacketLengthSize = sizeof(ushort);

        public const int ChecksumPos = PacketLengthPos + PacketLengthSize;
        public const int ChecksumSize = sizeof(uint);

        public const int SequenceNumberPos = PacketLengthPos + PacketLengthSize;
        public const int SequenceNumberSize = sizeof(uint);
        
        public class ConnectionStateEvent : UnityEvent { }
        public class ConnectionErrorEvent : UnityEvent<string, bool> { }
        public class ConnectionSocketErrorEvent : UnityEvent<SocketError, string> { }
        public class ConnectionCommunicationEvent : UnityEvent<CommunicationStream> { }

        protected Socket _socket = null;
        protected string _address = null;
        protected int _port = 0;
        protected bool _established = false;
        protected bool _terminated = false;
        protected bool _sending = false;
        protected bool _receiving = false;
        protected Queue _packetQueue = null;

        public ConnectionStateEvent onConnectionEstablished { get; } = new ConnectionStateEvent();
        public ConnectionStateEvent onConnectionTerminated { get; } = new ConnectionStateEvent();
        public ConnectionErrorEvent onConnectionError { get; } = new ConnectionErrorEvent();
        public ConnectionSocketErrorEvent onConnectionSocketError { get; } = new ConnectionSocketErrorEvent();
        public ConnectionCommunicationEvent onConnectionReceived { get; } = new ConnectionCommunicationEvent();
        public ConnectionCommunicationEvent onConnectionSent { get; } = new ConnectionCommunicationEvent();

        public bool Established { get => _established; }
        public bool Sending { get => _sending; }
        public bool Receiving { get => _receiving; }
        public bool Terminated { get => _terminated; }

        public void Connect(string address, int port) {
            if (_socket != null || _established)
                throw new InvalidOperationException("Connection.Connect: Trying to connect over an established connection.");
            
            _terminated = false;
            _address = address;
            _port = port;

            try {
                var addresses = Dns.GetHostAddresses(_address);
                if (addresses == null || addresses.Length == 0) {
                    onConnectionSocketError.Invoke(SocketError.AddressNotAvailable, "Invalid IP/Hostname given as a parameter.");
                    return;
                }

                var endPoint = new IPEndPoint(addresses[0], port);

                _socket = new Socket(endPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                var asyncResult = _socket.BeginConnect(endPoint, null, null);
                asyncResult.AsyncWaitHandle.WaitOne(Constants.ConnectionTimeout);
                OnConnectionConnected(asyncResult);
            } catch (SocketException e) {
                onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
            } catch (Exception e) {
                onConnectionSocketError.Invoke(SocketError.NotSocket, e.Message);
            }
        }

        public void Disconnect() {
            if (_terminated || !_established)
                return;
            _socket.Disconnect(false);
            HandleCommunicationTermination();
        }

        public void Send(CommunicationStream message) {
            if (_terminated || !_established)
                throw new InvalidOperationException("Connection.Send: Trying to send before connecting.");

            CommunicationStream clone;

            long position = message.Position;
            message.Position = 0;
            clone = new CommunicationStream(message);
            message.Position = position;

            lock (_packetQueue) {
                if (_sending) {
                    _packetQueue.Enqueue(clone);
                    return;
                }

                InternalSend(clone);
            }
        }

        public void Receive() {
            if (_terminated || !_established)
                throw new InvalidOperationException("Connection.Send: Trying to receive before connecting.");

            if (_receiving)
                return;
            
            _receiving = true;
            InternalReceiveHeader();
        }

        protected void InternalSend(CommunicationStream stream) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);
            InternalSend(buffer);
        }

        protected void InternalSend(byte[] buffer) {
            // this function is guaranteed to be called only
            // when the connection is established
            _sending = true;

            var stateObject = new AsyncStateHolder(buffer);
            _socket.BeginSend(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionSend, stateObject);
        }

        protected void InternalReceiveHeader() {
            // this function is guaranteed to be called only
            // when the connection is established
            var buffer = new byte[PacketLengthSize];
            var stateObject = new AsyncStateHolder(buffer);
            _socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvHeader, stateObject);
        }

        protected void InernalReceiveBody(int size) {
            // this function is guaranteed to be called only
            // when the connection is established
            var buffer = new byte[size];
            var stateObject = new AsyncStateHolder(buffer);
            _socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvBody, stateObject);
        }

        private void OnConnectionConnected(IAsyncResult asyncResult) {
            if (_terminated)
                return;
            
            try {
                _socket.EndConnect(asyncResult);
            } catch (SocketException e) {
                onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
                HandleCommunicationTermination();
                return;
            }

            _established = true;
            _packetQueue = new Queue();
            onConnectionEstablished.Invoke();
        }

        private void OnConnectionSend(IAsyncResult asyncResult) {
            if (_terminated)
                return;

            var stateObject = asyncResult.AsyncState as AsyncStateHolder;
            int total = 0;
            try {
                total = _socket.EndSend(asyncResult);
            } catch (SocketException e) {
                if (!_terminated) {
                    onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
                    HandleCommunicationTermination();
                }
                return;
            }

            if (total == 0) {
                HandleCommunicationTermination();
                return;
            }

            _sending = false;

            stateObject.State += total;
            if (stateObject.Finished) {
                onConnectionSent.Invoke(new CommunicationStream(stateObject.AsyncBuffer));
                lock (_packetQueue) {
                    if (_packetQueue.Count >= 0) {
                        InternalSend(_packetQueue.Dequeue() as CommunicationStream);
                        return;
                    }
                }

                return;
            }

            // send the packets left until the state object is finished
            _socket.BeginSend(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionSend, stateObject);
        }

        private void OnConnectionRecvHeader(IAsyncResult asyncResult) {
            if (_terminated)
                return;

            var stateObject = asyncResult.AsyncState as AsyncStateHolder;
            int total = 0;
            try {
                total = _socket.EndReceive(asyncResult);
            } catch (SocketException e) {
                if (!_terminated) {
                    onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
                    HandleCommunicationTermination();
                }
                return;
            }

            if (total == 0) {
                HandleCommunicationTermination();
                return;
            }

            stateObject.State += total;
            if (stateObject.Finished) {
                int size = BitConverter.ToUInt16(stateObject.Buffer, 0);
                InernalReceiveBody(size);
                return;
            }
            
            // keep receiving until the state object is finished
            _socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvHeader, stateObject);
        }

        private void OnConnectionRecvBody(IAsyncResult asyncResult) {
            if (_terminated)
                return;

            var stateObject = asyncResult.AsyncState as AsyncStateHolder;
            int total = 0;
            try {
                total = _socket.EndReceive(asyncResult);
            } catch (SocketException e) {
                if (!_terminated) {
                    onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
                    HandleCommunicationTermination();
                }
                return;
            }

            if (total == 0) {
                HandleCommunicationTermination();
                return;
            }

            stateObject.State += total;
            if (stateObject.Finished) {
                onConnectionReceived.Invoke(new CommunicationStream(stateObject.AsyncBuffer));

                _receiving = false;
                return;
            }

            // keep receiving until the state object is finished
            _socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvBody, stateObject);
        }

        private void HandleCommunicationTermination() {
            _terminated = true;
            _established = false;
            _receiving = false;
            _sending = false;

            if (_socket != null) {
                _socket.Dispose();
                _socket = null;
            }

            _packetQueue = null;

            onConnectionTerminated.Invoke();
        }

        public static bool operator !(Connection instance) {
            return instance == null;
        }

        public static bool operator true(Connection instance) {
            return !!instance;
        }

        public static bool operator false(Connection instance) {
            return !instance;
        }
    }
}
