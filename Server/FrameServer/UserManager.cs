using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer
{
    class UserManager
    {
        private int userID = 0;
        private Dictionary<string, int> dic_NameID = new Dictionary<string, int>();//用户名-ID的映射表
        private static readonly object _lock = new object();
        private static UserManager _instance;
        public static UserManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new UserManager();
                    }
                    return _instance;
                }
            }
        }

        private UserManager()
        {

        }

        public bool CheckLogin(string playerName)
        {
            foreach (var tcpclient in TCPServer.Instance.tcpClients)
            {
                if (playerName == tcpclient.playerName)
                {
                    return true;
                }
            }
            return false;
        }

        public int UserLogin(string playerName, TCPClientPeer tcpClient)
        {
            if (dic_NameID.ContainsKey(playerName) == false)
            {//从未登录过的新玩家

                userID++;
                dic_NameID.Add(playerName, userID);
                tcpClient.playerName = playerName;
                tcpClient.uid = userID;
                return tcpClient.uid;
            }
            else
            {
                //之前登录过的玩家
                tcpClient.playerName = playerName;
                tcpClient.uid = dic_NameID[playerName];
                return tcpClient.uid;
            }
        }

        public bool CheckReconnect(int uid)
        {
            if (BattleManager.Instance.dic_UidBattleid.ContainsKey(uid))
            {
                return true;
            }
            return false;
        }

        
    }
}
