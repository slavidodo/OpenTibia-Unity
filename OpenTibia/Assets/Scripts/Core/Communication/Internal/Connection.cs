using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    internal class AsyncStateHolder
    {
        internal ByteArray AsyncBuffer;
        internal int State = 0;

        internal int Required {
            get => AsyncBuffer.Length - State;
        }

        internal bool Finished {
            get => State == AsyncBuffer.Length;
        }

        internal byte[] Buffer {
            get => AsyncBuffer.Buffer;
        }

        internal AsyncStateHolder(ByteArray asyncBuffer, int state = 0) {
            AsyncBuffer = asyncBuffer;
            State = state;
        }
    }

    internal class Connection
    {
        internal const int HeaderPos = 0;

        internal const int PacketLengthPos = 0;
        internal const int PacketLengthSize = sizeof(ushort);

        internal const int ChecksumPos = PacketLengthPos + PacketLengthSize;
        internal const int ChecksumSize = sizeof(uint);

        internal const int SequenceNumberPos = PacketLengthPos + PacketLengthSize;
        internal const int SequenceNumberSize = sizeof(uint);
        
        internal class ConnectionStateEvent : UnityEvent { }
        internal class ConnectionErrorEvent : UnityEvent<string, bool> { }
        internal class ConnectionSocketErrorEvent : UnityEvent<SocketError, string> { }
        internal class ConnectionCommunicationEvent : UnityEvent<ByteArray> { }

        protected Socket m_Socket = null;
        protected string m_Address = null;
        protected int m_Port = 0;
        protected bool m_Established = false;
        protected bool m_Terminated = false;
        protected bool m_Sending = false;
        protected bool m_Receiving = false;
        protected Queue m_PacketQueue = null;

        internal ConnectionStateEvent onConnectionEstablished { get; } = new ConnectionStateEvent();
        internal ConnectionStateEvent onConnectionTerminated { get; } = new ConnectionStateEvent();
        internal ConnectionErrorEvent onConnectionError { get; } = new ConnectionErrorEvent();
        internal ConnectionSocketErrorEvent onConnectionSocketError { get; } = new ConnectionSocketErrorEvent();
        internal ConnectionCommunicationEvent onConnectionReceived { get; } = new ConnectionCommunicationEvent();
        internal ConnectionCommunicationEvent onConnectionSent { get; } = new ConnectionCommunicationEvent();

        internal bool Established { get => m_Established; }
        internal bool Sending { get => m_Sending; }
        internal bool Receiving { get => m_Receiving; }
        internal bool Terminated { get => m_Terminated; }

        internal void Connect(string address, int port) {
            if (m_Socket != null || m_Established)
                throw new InvalidOperationException("Connection.Connect: Trying to connect over an established connection.");
            
            m_Terminated = false;
            m_Address = address;
            m_Port = port;
            
            var addresses = Dns.GetHostAddresses(m_Address);
            if (addresses == null || addresses.Length == 0) {
                onConnectionSocketError.Invoke(SocketError.AddressNotAvailable, "Invalid IP/Hostname given as a parameter.");
                return;
            }
            
            var endPoint = new IPEndPoint(addresses[0], port);
            m_Socket = new Socket(endPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                var asyncResult = m_Socket.BeginConnect(endPoint, null, null);
                asyncResult.AsyncWaitHandle.WaitOne(Constants.ConnectionTimeout);
                OnConnectionConnected(asyncResult);
            } catch (SocketException e) {
                onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
            }
        }

        internal void Disconnect() {
            if (m_Terminated || !m_Established)
                return;
            
            m_Socket.Disconnect(false);
            HandleCommunicationTermination();
        }

        internal void Send(ByteArray message) {
            if (m_Terminated || !m_Established)
                throw new InvalidOperationException("Connection.Send: Trying to send before connecting.");

            var clone = message.Clone();
            lock (m_PacketQueue) {
                if (m_Sending) {
                    m_PacketQueue.Enqueue(clone);
                    return;
                }

                InternalSend(clone);
            }
        }

        internal void Receive() {
            if (m_Terminated || !m_Established)
                throw new InvalidOperationException("Connection.Send: Trying to receive before connecting.");

            if (m_Receiving)
                return;
            
            m_Receiving = true;
            InternalReceiveHeader();
        }

        protected void InternalSend(ByteArray message) {
            // this function is guaranteed to be called only
            // when the connection is established
            m_Sending = true;

            var stateObject = new AsyncStateHolder(message);
            m_Socket.BeginSend(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionSend, stateObject);
        }

        protected void InternalReceiveHeader() {
            // this function is guaranteed to be called only
            // when the connection is established
            var buffer = new byte[PacketLengthSize];
            var byteArray = new ByteArray(buffer);
            var stateObject = new AsyncStateHolder(byteArray);
            m_Socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvHeader, stateObject);
        }

        protected void InernalReceiveBody(int size) {
            // this function is guaranteed to be called only
            // when the connection is established
            var buffer = new byte[size];
            var byteArray = new ByteArray(buffer);
            var stateObject = new AsyncStateHolder(byteArray);
            m_Socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvBody, stateObject);
        }

        private void OnConnectionConnected(IAsyncResult asyncResult) {
            if (m_Terminated)
                return;
            
            try {
                m_Socket.EndConnect(asyncResult);
            } catch (SocketException e) {
                onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
                HandleCommunicationTermination();
                return;
            }

            m_Established = true;
            m_PacketQueue = new Queue();
            onConnectionEstablished.Invoke();
        }

        private void OnConnectionSend(IAsyncResult asyncResult) {
            if (m_Terminated)
                return;

            var stateObject = asyncResult.AsyncState as AsyncStateHolder;
            int total = 0;
            try {
                total = m_Socket.EndSend(asyncResult);
            } catch (SocketException e) {
                if (!m_Terminated) {
                    onConnectionSocketError.Invoke(e.SocketErrorCode, e.Message);
                    HandleCommunicationTermination();
                }
                return;
            }
            
            if (total == 0) {
                HandleCommunicationTermination();
                return;
            }

            m_Sending = false;

            stateObject.State += total;
            if (stateObject.Finished) {
                onConnectionSent.Invoke(stateObject.AsyncBuffer);
                lock (m_PacketQueue) {
                    if (m_PacketQueue.Count >= 0) {
                        InternalSend(m_PacketQueue.Dequeue() as ByteArray);
                        return;
                    }
                }

                return;
            }

            // send the packets left until the state object is finished
            m_Socket.BeginSend(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionSend, stateObject);
        }

        private void OnConnectionRecvHeader(IAsyncResult asyncResult) {
            if (m_Terminated)
                return;

            var stateObject = asyncResult.AsyncState as AsyncStateHolder;
            int total = 0;
            try {
                total = m_Socket.EndReceive(asyncResult);
            } catch (SocketException e) {
                if (!m_Terminated) {
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
            m_Socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvHeader, stateObject);
        }

        private void OnConnectionRecvBody(IAsyncResult asyncResult) {
            if (m_Terminated)
                return;

            var stateObject = asyncResult.AsyncState as AsyncStateHolder;
            int total = 0;
            try {
                total = m_Socket.EndReceive(asyncResult);
            } catch (SocketException e) {
                if (!m_Terminated) {
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
                onConnectionReceived.Invoke(stateObject.AsyncBuffer);

                m_Receiving = false;
                return;
            }

            // keep receiving until the state object is finished
            m_Socket.BeginReceive(stateObject.Buffer, stateObject.State, stateObject.Required, SocketFlags.None, OnConnectionRecvBody, stateObject);
        }

        private void HandleCommunicationTermination() {
            m_Terminated = true;
            m_Established = false;
            m_Receiving = false;
            m_Sending = false;

            m_Socket.Dispose();
            m_Socket = null;
            m_PacketQueue = null;

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
