using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace OpenTibiaUnity.Core.Network
{
    public class Connection
    {
        public delegate void OnConnectionConnect();
        public delegate void OnConnectionDisconnect();
        public delegate void OnConnectionError(SocketError code, string message);
        public delegate void OnConnectionRecv(InputMessage message);
        public delegate void OnSendMessage(OutputMessage message);

        public const int HEADER_LENGTH = 2;

        Socket m_Socket;
        Queue m_MessageQueue;
        readonly object m_Mutex;

        private OnConnectionConnect m_OnConnectionConnect;
        private OnConnectionDisconnect m_OnConnectionDisconnect;
        private OnConnectionError m_OnConnectionError;
        private OnConnectionRecv m_OnConnectionRecv;
        private OnSendMessage m_OnSendMessage;
        
        public bool IsConnected {
            get {
                return m_Socket != null ? m_Socket.Connected : false;
            }
        }

        public Connection(OnConnectionConnect onConnected = null,
                            OnConnectionDisconnect onDisconnected = null,
                            OnConnectionError onError = null,
                            OnConnectionRecv onRecv = null,
                            OnSendMessage onSendMessage = null) {
            m_OnConnectionConnect = onConnected;
            m_OnConnectionDisconnect = onDisconnected;
            m_OnConnectionError = onError;
            m_OnConnectionRecv = onRecv;
            m_OnSendMessage = onSendMessage;

            m_MessageQueue = new Queue();
            m_Mutex = new object();
        }
        
        public void Connect(string ip, int port) {
            try {
                var addresses = Dns.GetHostAddresses(ip);
                if (addresses == null || addresses.Length != 1) {
                    OnError(SocketError.InvalidArgument, "Invalid IP/Hostname given as a parameter.");
                    return;
                }

                IPEndPoint iPEndPoint = new IPEndPoint(addresses[0], port);
                m_Socket = new Socket(iPEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                m_Socket.BeginConnect(iPEndPoint, OnConnected, m_Socket);
            } catch (SocketException e) {
                OnError(e.SocketErrorCode, e.Message);
            }
        }
        
        public void Disconnect(bool reuse = false) {
            m_OnConnectionConnect = null;
            m_OnConnectionDisconnect = null;
            m_OnConnectionError = null;
            m_OnConnectionRecv = null;

            if (m_Socket?.Connected == true) {
                try {
                    m_Socket.BeginDisconnect(reuse, OnDisconnected, null);
                } catch (SocketException) {}
            }

            if (!reuse) {
                m_Socket.Dispose();
                m_Socket = null;
            }
        }

        public void Send(OutputMessage message) {
            if (m_Socket?.Connected != true) {
                return;
            }

            bool empty = m_MessageQueue.Count == 0;
            m_MessageQueue.Enqueue(message);
            
            if (empty)
                internalSend(message);
        }

        void internalSend(OutputMessage message) {
            try {
                m_OnSendMessage?.Invoke(message);
                m_Socket.BeginSend(message.GetBufferArray(), 0, message.GetBufferLength(), SocketFlags.None, OnSend, m_Socket);
            } catch (SocketException e) {
                OnError(e.SocketErrorCode, e.Message);
            }
        }

        public void BeginRecv(int n = HEADER_LENGTH, StateObject so = null) {
            if (m_Socket?.Connected != true)
                return;

            if (so == null)
                so = new StateObject(n);

            try {
                m_Socket.BeginReceive(so.buffer, so.readSize, n, SocketFlags.None, OnRecvHeader, so);
            } catch (SocketException e) {
                OnError(e.SocketErrorCode, e.Message);
            }
        }

        // Hooks
        protected void OnConnected(IAsyncResult result) {
            try {
                m_Socket?.EndConnect(result);
            } catch (SocketException e) {
                m_OnConnectionError?.Invoke(e.SocketErrorCode, e.Message);
                return;
            }
           
            m_OnConnectionConnect?.Invoke();
        }

        protected void OnDisconnected(IAsyncResult result) {
            try {
                m_Socket?.EndDisconnect(result);
            } catch (SocketException e) {
                m_OnConnectionError?.Invoke(e.SocketErrorCode, e.Message);
                return;
            }

            m_OnConnectionDisconnect?.Invoke();
        }

        protected void OnError(SocketError code, string message) {
            m_OnConnectionError?.Invoke(code, message);
            Disconnect();
        }

        protected void OnSend(IAsyncResult result) {
            try {
                m_Socket.EndSend(result);
            } catch (SocketException e) {
                OnError(e.SocketErrorCode, e.Message);
                return;
            }
            
            m_MessageQueue.Dequeue();
            if (m_MessageQueue.Count > 0)
                internalSend((OutputMessage)m_MessageQueue.Peek());
        }

        protected void OnRecvHeader(IAsyncResult result) {
            if (m_Socket?.Connected != true)
                return;

            int read;
            try {
                read = m_Socket.EndReceive(result);
            } catch (SocketException e) {
                OnError(e.SocketErrorCode, e.Message);
                return;
            }
            
            if (read == 0) {
                m_OnConnectionDisconnect?.Invoke();
                return;
            }

            var so = (StateObject)result.AsyncState;
            so.readSize += read;
            if (so.readSize < so.totalSize) {
                BeginRecv(so.totalSize - so.readSize, so);
                return;
            }

            var buffer = so.buffer;
            var headerLength = buffer[0] | buffer[1] << 8; ;
            if (headerLength > 0) {
                so = new StateObject(headerLength);
                m_Socket.BeginReceive(so.buffer, 0, headerLength, SocketFlags.None, OnRecvPacket, so);
            } else {
                m_OnConnectionDisconnect?.Invoke();
            }
        }

        protected void OnRecvPacket(IAsyncResult result) {
            int read;
            try {
                read = m_Socket.EndReceive(result);
            } catch (SocketException e) {
                OnError(e.SocketErrorCode, e.Message);
                return;
            }

            if (read == 0) {
                m_OnConnectionDisconnect?.Invoke();
                return;
            }

            var so = (StateObject)result.AsyncState;
            so.readSize += read;
            if (so.readSize < so.totalSize) {
                m_Socket.BeginReceive(so.buffer, so.readSize, so.totalSize - so.readSize, SocketFlags.None, OnRecvPacket, so);
                return;
            }

            BeginRecv();
            lock (m_Mutex) {
                InputMessage message = new InputMessage(so.buffer);
                m_OnConnectionRecv?.Invoke(message);
            }
        }
    }

    public class StateObject
    {
        public StateObject(int n) {
            buffer = new byte[n];
            totalSize = n;
        }

        public byte[] buffer;

        public int readSize = 0;
        public int totalSize = 0;

        public Socket socket;
    }
}