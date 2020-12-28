using FrameServer.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace FrameServer
{
    class TCPClientPeer
    {
        public Socket clientSocket;
        private Message message;

        public string playerName;
        public int uid;
        public IPEndPoint clientUpdEnd;
        public TCPClientPeer(Socket clientSocket)
        {
            Console.Write("一个客户端连接~");
            this.clientSocket = clientSocket;
            TCPServer.Instance.tcpClients.Add(this);
            Console.WriteLine("当前连接数：" + TCPServer.Instance.tcpClients.Count);
            message = new Message();
            clientSocket.BeginReceive(message.buffer, message.Start, message.Remain, SocketFlags.None, ReceiveCallBack, null);
        }

        private void ReceiveCallBack(IAsyncResult asyncResult) {
            try
            {
                int count = clientSocket.EndReceive(asyncResult);
                if (count <= 0)
                {
                    Console.WriteLine("tcp连接释放，客户端主动断开连接");
                    TCPServer.Instance.tcpClients.Remove(this);
                    clientSocket.Close();
                    return;
                }
                message.UnpackMessage(count, HandleRequest);

                clientSocket.BeginReceive(message.buffer, message.Start, message.Remain, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception e){
                Console.WriteLine(e.Message+e.StackTrace);
                if(clientSocket != null)
                {
                    Console.WriteLine("tcp连接释放，客户端强迫断开连接");
                    TCPServer.Instance.tcpClients.Remove(this);
                    clientSocket.Close();
                }
            }
        }

        private void HandleRequest(byte requestByte, byte[] parameters)
        {            
            PB_RequestCode requestCode = (PB_RequestCode)requestByte;
            Console.WriteLine("<--tcp:"+((IPEndPoint)clientSocket.RemoteEndPoint).ToString() + ":" + requestCode.ToString());
            switch (requestCode)
            {
                case PB_RequestCode.Login:
                    {
                        LoginHandler loginHandler = new LoginHandler();
                        loginHandler.HandlerRequest<Login>(Login.Parser.ParseFrom(parameters), this);
                        break;
                    }
                case PB_RequestCode.Match:
                    {
                        MatchHandler matchHandler = new MatchHandler();
                        matchHandler.HandlerRequest<Match>(Match.Parser.ParseFrom(parameters), this);
                        break;
                    }

                case PB_RequestCode.MatchCancle:
                    {
                        MatchCancleHandler matchCancleHandler = new MatchCancleHandler();
                        matchCancleHandler.HandlerRequest<MatchCancle>(MatchCancle.Parser.ParseFrom(parameters), this);
                        break;
                    }
                case PB_RequestCode.Reconnect:
                    {
                        Reconnect reconnect = Message.Deserialize<Reconnect>(parameters);
                        int uid = reconnect.Uid;
                        bool res = UserManager.Instance.CheckReconnect(uid);
                        if (res == false)
                        {
                            ReconnectResponse reconnectResponse = new ReconnectResponse { Result = false };
                            this.SendMessage<ReconnectResponse>(PB_ResponseCode.ReconnectResponse, reconnectResponse);
                        }
                        else
                        {
                            this.clientUpdEnd = new IPEndPoint(IPAddress.Parse(reconnect.ClientIP), reconnect.ClientUdpPort);
                            BattleManager.Instance.Reconnect(uid, this);
                        }
                        break;
                    }
            }

        }

        //private void HandleRequest_Reflection(byte requestByte, byte[] parameters)
        //{
        //    PB_RequestCode requestCode = (PB_RequestCode)requestByte;
        //    classname： FrameServer.Controller.LoginHandler
        //    string clsName = $"FrameServer.Controller.{requestCode.ToString()}Handler";
        //    Console.WriteLine("clsName: " + clsName);
        //    Type type = Type.GetType(clsName);
        //    Message.Deserialize<>
        //    MethodInfo methodInfo = type.GetMethod("HandleRequest");
        //    object clsInstance = Activator.CreateInstance(type);
        //    object[] methodParams = new object[] { requestCode, };
        //}

        public void SendMessage<T>(PB_ResponseCode responseCode, T parameter)
        {
            try
            {
                Console.WriteLine("-->tcp:" + ((IPEndPoint)clientSocket.RemoteEndPoint).ToString() + ":" + responseCode.ToString()+":"+parameter.ToString());
                byte[] data = Message.PackMessage<T>(responseCode, parameter);
                clientSocket.Send(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
            }
        }
    }
}
