using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace FrameServer
{
    class UDPServer
    {
        public UdpClient udpClient;
        public int udpRcvPort;
        private bool isRun = false;

        //public Dictionary<int, UDPClientPeer> dic_udpClients = new Dictionary<int, UDPClientPeer>();//uid - udpClientPeer

        private static readonly object _lock = new object();
        private static UDPServer _instance;
        public static UDPServer Instance
        {
            get
            {
                lock (_lock)
                {
                    if(_instance == null)
                    {
                        _instance = new UDPServer();
                    }
                    return _instance;
                }
            }
        }

        private UDPServer()
        {
        }

        public void StartServer()
        {
            udpClient = new UdpClient(ServerConfig.udpPort);
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            isRun = true;
            IPEndPoint iPEndPoint = udpClient.Client.LocalEndPoint as IPEndPoint;
            udpRcvPort = iPEndPoint.Port;
            CreateNewRcvThread();
        }

        public UdpClient GetUdpClient()
        {
            if(udpClient == null)
            {
                StartServer();
            }
            return udpClient;
        }

        public void CreateNewRcvThread()
        {
            ThreadPool.QueueUserWorkItem((obj) => { RcvThread(); });           
        }

        private void RcvThread()
        {
            Console.WriteLine($"ThreadID：{Thread.CurrentThread.ManagedThreadId}---udp接收线程被--创建--");

            while (isRun)
            {
                try
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = udpClient.Receive(ref iPEndPoint);
                    //Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    if (buffer.Length <= 6)
                    {
                        Console.WriteLine($"ThreadID：{Thread.CurrentThread.ManagedThreadId}---接受的udp数据长度小于6");
                        continue;
                    }
                    int count = BitConverter.ToInt32(buffer, 0);
                    //Console.WriteLine(count + "-------------");
                    byte requestCode = buffer[4];
                    byte[] parameters = new byte[count - 1];
                    try
                    {
                        Array.Copy(buffer, 5, parameters, 0, count - 1);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e);
                        foreach (var item in buffer)
                        {
                            Console.Write(item + ":");
                        }
                        Console.WriteLine();
                        //Console.WriteLine($"buffer.length:{buffer.Length}---------------");
                        //Console.WriteLine($"count.length:{count}---------------");
                        //Console.WriteLine($"parameters.length:{parameters.Length}---------------");
                        continue;
                    }
                    this.AnalyzeMessage(requestCode, parameters, iPEndPoint);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                }
            }
            Console.WriteLine($"ThreadID：{Thread.CurrentThread.ManagedThreadId}---udp接收线程被xx销毁xx");
        }

        private void AnalyzeMessage(byte requestCode, byte[] parameters, IPEndPoint iPEndPoint)
        {
            PB_RequestCode request = (PB_RequestCode)requestCode;
            //Console.WriteLine("<--udp:" + clientEndPoint.ToString() + ":" + request.ToString());
            switch (request)
            {
                case PB_RequestCode.BattleReady:
                    {
                        BattleReady battleReady = Message.Deserialize<BattleReady>(parameters);
                        int battleID = battleReady.BattleUserInfo.BattleID;
                        int uid = battleReady.BattleUserInfo.Uid;

                        //dic_udpClients[uid].clientEndPoint = iPEndPoint;

                        BattleManager.Instance.RcvBattleReady(battleID, uid);
                        break;
                    }
                case PB_RequestCode.UpPlayerOperation:
                    {
                        UpPlayerOperation upPlayerOperation = Message.Deserialize<UpPlayerOperation>(parameters);
                        int messageID = upPlayerOperation.MesID;
                        PlayerOperation playerOperation = upPlayerOperation.PlayerOperation;
                        BattleManager.Instance.RcvUpPlayerOperation(upPlayerOperation);
                        break;
                    }

                case PB_RequestCode.RequestLackFrames:
                    {
                        RequestLackFrames requestLackFrames = Message.Deserialize<RequestLackFrames>(parameters);
                        BattleManager.Instance.RcvRequestLackFrames(requestLackFrames);
                        break;
                    }

                case PB_RequestCode.RequestGameOver:
                    {
                        RequestGameOver requestGameOver = Message.Deserialize<RequestGameOver>(parameters);
                        BattleManager.Instance.RcvRequestGameOver(requestGameOver);
                        break;
                    }

            }
        }

    }
}
