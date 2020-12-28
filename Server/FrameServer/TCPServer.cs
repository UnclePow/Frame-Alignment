using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace FrameServer
{
    
    class TCPServer
    {
        public Socket serverSocket;
        public List<TCPClientPeer> tcpClients = new List<TCPClientPeer>();
        private static readonly object stLockObj = new object();

        private static TCPServer _instance;
        public static TCPServer Instance
        {
            get {
                lock (stLockObj)
                {
                    if (_instance == null) {
                        _instance = new TCPServer();
                    }
                    return _instance;
                }
            }
        }
        private TCPServer() { 
            
        }

        public void StartServer()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ServerConfig.serverIP), ServerConfig.tcpPort);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(iPEndPoint);
            serverSocket.Listen(10);
            serverSocket.BeginAccept(AcceptCallBack, null);
            Console.WriteLine("服务器启动成功...");
        }

        public void StartServer(string serverIP, int tcpPort)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), tcpPort);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(iPEndPoint);
            serverSocket.Listen(10);
            serverSocket.BeginAccept(AcceptCallBack, null);
        }

        private void AcceptCallBack(IAsyncResult asyncResult)
        {
            Socket clientSocket = serverSocket.EndAccept(asyncResult);
            TCPClientPeer client = new TCPClientPeer(clientSocket);
            serverSocket.BeginAccept(AcceptCallBack, null);
        }
    }
}
