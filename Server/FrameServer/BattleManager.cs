using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer
{
    class BattleManager
    {
        private int curBattleID = 0;

        //在进入战局时新增，在正式结束时移除
        private Dictionary<int, Battle> battleDict = new Dictionary<int, Battle>();//battleID-battle

        //用来记录玩家在战局中的情况，一般用于断线重连  在进入战局时新增，在正式结束时移除
        public Dictionary<int, int> dic_UidBattleid = new Dictionary<int, int>();//userID-battleID 
        

        private static readonly object _lock = new object();
        private static BattleManager _instance;
        public static BattleManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if(_instance == null)
                    {
                        _instance = new BattleManager();
                    }
                    return _instance;
                }
            }
        }

        private BattleManager()
        {

        }

        public void BeginCreateBattle(List<TCPClientPeer> confirmedClients)
        {
            curBattleID++;
            Battle battle = new Battle();
            battle.CreateBattle(curBattleID, confirmedClients, new Random().Next(1, 100));

            AddBattle(battle);
            foreach(var item in confirmedClients)
            {
                AddUserToBattle(item.uid, curBattleID);
            }

        }

        public void AddBattle(Battle battle)
        {
            this.battleDict.Add(battle.battleID, battle);
        }

        

        public bool RemoveBattle(int battleID)
        {
            bool result = this.battleDict.Remove(battleID);
            return result;
        }

        public void AddUserToBattle(int uid, int battleID)
        {
            this.dic_UidBattleid.Add(uid, battleID);
        }

        public void RemoveUser(int uid)
        {
            this.dic_UidBattleid.Remove(uid);
        }

        public void RcvBattleReady(int battleID, int uid)
        {
            if(battleDict.ContainsKey(battleID) == false)
            {
                Console.WriteLine("对局已失效---------");
                return;
            }
            battleDict.TryGetValue(battleID, out Battle battle);
            battle.RcvReady(uid);

        }

        public void RcvUpPlayerOperation(UpPlayerOperation upPlayerOperation)
        {
            PlayerOperation playerOperation = upPlayerOperation.PlayerOperation;
            int battleID = playerOperation.BattleUserInfo.BattleID;
            if (battleDict.ContainsKey(battleID) == false)
            {
                Console.WriteLine("对局已失效---------");
                return;
            }
            battleDict.TryGetValue(battleID, out Battle battle);
            battle.RcvPlayerOperation(upPlayerOperation);
        }

        public void RcvRequestLackFrames(RequestLackFrames requestLackFrames)
        {
            int battleID = requestLackFrames.BattleUserInfo.BattleID;
            if (battleDict.ContainsKey(battleID) == false)
            {
                Console.WriteLine("对局已失效---------");
                return;
            }
            battleDict.TryGetValue(battleID, out Battle battle);
            battle.RcvRequestLackFrames(requestLackFrames);
        }

        public void RcvRequestGameOver(RequestGameOver requestGameOver)
        {
            int battleID = requestGameOver.BattleUserInfo.BattleID;
            if (battleDict.ContainsKey(battleID) == false)
            {
                Console.WriteLine("对局已失效---------");
                return;
            }
            battleDict.TryGetValue(battleID, out Battle battle);
            battle.RcvGameOver(requestGameOver);
        }

        public void Reconnect(int uid, TCPClientPeer tcpClient)
        {
            int battleID = dic_UidBattleid[uid];
            if (battleDict.ContainsKey(battleID) == false)
            {
                Console.WriteLine("对局已失效---------");
                return;
            }
            battleDict.TryGetValue(battleID, out Battle battle);
            battle.RcvReconnect(uid, tcpClient);
        }
    }
}
