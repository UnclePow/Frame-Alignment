using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameServer
{
    class Battle
    {
        public int battleID;
        public Dictionary<int, TCPClientPeer> dic_tcp;
        public int randomSeed;

        private Dictionary<int, UDPClientPeer> dic_udp;//userid-clientudp

        private Dictionary<int, bool> dic_battleReady;//userid-ready

        private bool _isRun = false;
        private bool isBeginBattle = false;
        private int frameNum;
        private int lastFrame;

        private static readonly object _lock = new object();

        private Dictionary<int, PlayerOperation> dic_FrameOperation;
        //private PlayerOperation[] frameOperation;//记录当前帧的所有玩家操作，数组中的每个元素代表每个玩家的帧操作
        private Dictionary<int, int> dic_playerMesNum;
        //private int[] playerMesNum;//记录玩家的包id
        private bool[] playerGameOver;//记录玩家游戏结束
        private bool oneGameOver;
        private bool allGameOver;

        private Dictionary<int, List<PlayerOperation>> dic_gameOperation = new Dictionary<int, List<PlayerOperation>>();
        //frameNum-List<PlayerOperation> 用来记录一局中所有玩家的帧操作


        public void CreateBattle(int battleID, List<TCPClientPeer> clientsInGame, int randomSeed)
        {
            this.battleID = battleID;
            this.dic_tcp = new Dictionary<int, TCPClientPeer>();
            foreach(var item in clientsInGame)
            {
                dic_tcp.Add(item.uid, item);
            }

            this.randomSeed = randomSeed;

            dic_udp = new Dictionary<int, UDPClientPeer>();
            dic_battleReady = new Dictionary<int, bool>();

            BattleEnter battleEnter = new BattleEnter();
            foreach(var tcpclient in clientsInGame)
            {
                UDPClientPeer udpClientPeer = new UDPClientPeer(tcpclient.uid, tcpclient.clientUpdEnd);
                dic_udp.Add(tcpclient.uid, udpClientPeer);
                dic_battleReady.Add(tcpclient.uid, false);
                //UDPServer.Instance.dic_udpClients.Add(tcpclient.uid, udpClientPeer);
                UDPServer.Instance.CreateNewRcvThread();

                BattleUserInfo battleUserInfo = new BattleUserInfo
                {
                    Uid = tcpclient.uid,
                    BattleID = battleID
                };
                battleEnter.AllPlayers.Add(battleUserInfo);
            }


            //send message
            foreach(var tcpclient in clientsInGame)
            {
                battleEnter.Seed = randomSeed;
                battleEnter.BattleID = battleID;
                tcpclient.SendMessage<BattleEnter>(PB_ResponseCode.BattleEnter, battleEnter);
                //Console.WriteLine("-->tcp" + ((IPEndPoint)tcpclient.clientSocket.RemoteEndPoint).ToString() + ":BattleEnter");
            }
        }

        //接收到了玩家准备好的消息
        public void RcvReady(int uid)
        {           
            if (isBeginBattle)
            {
                Console.WriteLine($"battleID:{battleID}-对局已经开始---------");
                return;
            }

            if (dic_battleReady.ContainsKey(uid) == false)
            {
                Console.WriteLine($"battleID:{battleID}-无效的uid-----------");
                return;
            }
            dic_battleReady[uid] = true;
            bool allReady = true;
            foreach(bool value in dic_battleReady.Values)
            {
                if (value == false)
                {
                    allReady = false;
                    break;
                }
            }
            if(allReady == true)
            {
                isBeginBattle = true;
                ThreadPool.QueueUserWorkItem((obj)=> { BeginBattle(); }, null);
            }
        }

        //所有的玩家都已经准备好啦~初始化一些数据，并告诉他们可以上传帧数据啦~~
        private void BeginBattle()
        {
            frameNum = 0;
            lastFrame = 0;
            _isRun = true;

            int playerNum = dic_udp.Keys.Count;

            //frameOperation = new PlayerOperation[playerNum];
            dic_FrameOperation = new Dictionary<int, PlayerOperation>();
            //playerMesNum = new int[playerNum];
            dic_playerMesNum = new Dictionary<int, int>();
            for (int i = 0; i < playerNum; i++)
            {
                //dic_FrameOperation[i] = null;
                //playerMesNum[i] = 0;
                dic_FrameOperation[ dic_tcp.Keys.ToList()[i] ] = null;
                dic_playerMesNum[ dic_tcp.Keys.ToList()[i] ] = 0;
            }

            //=======================================
            //通知所有玩家发送帧数据，同时等待它们上传的帧数据
            bool isFinishBS = false;
            while (!isFinishBS)
            {//在网络状况良好的前提下，此次循环只会执行两次
                BattleStart battleStart = new BattleStart();
                //向该battle中的所有玩家发送
                foreach (var udpclient in dic_udp)
                {
                    udpclient.Value.SendMessage<BattleStart>(PB_ResponseCode.BattleStart, battleStart);
                }

                bool _allData = true;
                //for (int i = 0; i < frameOperation.Length; i++)
                //{
                //    if (frameOperation[i] == null)
                //    {
                //        _allData = false;// 有一个玩家没有发送上来操作 则判断为false
                //        break;
                //    }
                //}
                foreach(PlayerOperation playerOperation in dic_FrameOperation.Values)
                {
                    if(playerOperation == null)
                    {
                        _allData = false;
                        break;
                    }
                }

                if (_allData)
                {
                    Console.WriteLine($"battleID:{battleID}-收到全部玩家的第一次操作数据~马上广播帧数据");
                    frameNum = 1;

                    isFinishBS = true;
                }

                Thread.Sleep(1000);
            }

            Thread _threadSenfd = new Thread(Thread_SendFrameData);
            _threadSenfd.Start();
        }

        private void Thread_SendFrameData()
        {
            Console.WriteLine($"battleID:{battleID}-开始广播帧数据---------------");

            while (_isRun)
            {
                DownPlayerOperations frameData = new DownPlayerOperations();
                //if (oneGameOver)//TODO
                //{
                //    frameData.frameID = lastFrame;
                //    frameData.operations = dic_gameOperation[lastFrame];
                //}
                //else
                //{
                dic_gameOperation[frameNum] = new List<PlayerOperation>(dic_FrameOperation.Values);
                //frameData.PlayerOperations.AddRange(dic_gameOperation[frameNum]);

                lock (_lock)
                {
                    foreach (PlayerOperation value in dic_FrameOperation.Values)
                        frameData.PlayerOperations.Add(value); 
                }         
                //frameData.PlayerOperations.Add(dic_FrameOperation.Values);
                frameData.FrameID = frameNum;

                lastFrame = frameNum;
                frameNum++;
                //}

                foreach (var item in dic_udp)
                {
                    item.Value.SendMessage<DownPlayerOperations>(PB_ResponseCode.DownPlayerOperations, frameData);
                }

                Thread.Sleep(ServerConfig.frameTime);
            }

            Console.WriteLine($"battleID:{battleID}-帧数据发送线程结束.....................");
        }

        public void RcvPlayerOperation(UpPlayerOperation upPlayerOperation)
        {
            int mesID = upPlayerOperation.MesID;
            PlayerOperation playerOperation = upPlayerOperation.PlayerOperation;
            int uid = playerOperation.BattleUserInfo.Uid;
            if (dic_udp.ContainsKey(uid) == false)
            {
                Console.WriteLine($"battleID:{battleID}-无效的uid-----------");
                return;
            }
            lock (_lock)
            {
                dic_FrameOperation[uid] = playerOperation;
            }
            dic_playerMesNum[uid] = mesID;
        }

        public void RcvRequestLackFrames(RequestLackFrames requestLackFrames)
        {
            int uid = requestLackFrames.BattleUserInfo.Uid;
            if (dic_udp.ContainsKey(uid) == false)
            {
                Console.WriteLine($"battleID:{battleID}-无效的uid-----------");
                return;
            }
            List<int> lackFrameIDs = new List<int>(requestLackFrames.LackFrameIDs);
            ResponseLackFrames responseLackFrames = new ResponseLackFrames();
            foreach(int frameID in lackFrameIDs)
            {
                DownPlayerOperations downFrameData = new DownPlayerOperations();
                downFrameData.FrameID = frameID;
                downFrameData.PlayerOperations.AddRange(dic_gameOperation[frameID]);
                responseLackFrames.LackFrames.Add(downFrameData);
            }

            dic_udp[uid].SendMessage<ResponseLackFrames>(PB_ResponseCode.ResponseLackFrames, responseLackFrames);
        }

        public void RcvGameOver(RequestGameOver requestGameOver)
        {
            foreach(var item in dic_udp)
            {
                ResponseGameOver responseGameOver = new ResponseGameOver();
                item.Value.SendMessage<ResponseGameOver>(PB_ResponseCode.ResponseGameOver, responseGameOver);
            }


            if (_isRun == false)
                return;
            _isRun = false;
            
            foreach(var item in dic_udp)
            {
                //UDPServer.Instance.dic_udpClients.Remove(item.Key);
                BattleManager.Instance.RemoveUser(item.Key);
            }
            BattleManager.Instance.RemoveBattle(this.battleID);
        }

        public void RcvReconnect(int uid, TCPClientPeer tcpClient)
        {
            dic_tcp[uid] = tcpClient;
            UDPClientPeer udpClient = new UDPClientPeer(uid, tcpClient.clientUpdEnd);
            dic_udp[uid] = udpClient;

            ReconnectResponse reconnectResponse = new ReconnectResponse {
                Result = true,
                BattleID = this.battleID,
                Seed = this.randomSeed
            };
            List<BattleUserInfo> battleUsers = new List<BattleUserInfo>();
            foreach(int key in dic_tcp.Keys)
                battleUsers.Add(new BattleUserInfo { Uid = key, BattleID = this.battleID });

            reconnectResponse.AllPlayers.AddRange(battleUsers);

            tcpClient.SendMessage<ReconnectResponse>(PB_ResponseCode.ReconnectResponse, reconnectResponse);
        }
    }
}
