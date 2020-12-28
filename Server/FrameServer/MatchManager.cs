using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer
{
    class MatchManager
    {
        private int maxNum = 1;
        private List<TCPClientPeer> _clientsInQueue = new List<TCPClientPeer>();
        private List<TCPClientPeer> ClientsInQueue
        {
            get
            {
                return _clientsInQueue;
            }
            set
            {
                lock (_lock)
                {
                    _clientsInQueue = value;
                }
            }
        }

        private static readonly object _lock = new object();
        private static MatchManager _instance;
        public static MatchManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MatchManager();                
                    }
                    return _instance;
                }
            }
        }

        private MatchManager()
        {

        }

        public void AddToMatchQueue(TCPClientPeer clientPeer)
        {
            ClientsInQueue.Add(clientPeer);
            
            if(ClientsInQueue.Count >= maxNum)
            {
                //Console.WriteLine(ClientsInQueue[0].playerName);
                List<TCPClientPeer> confirmedClients = new List<TCPClientPeer>();
                for(int i = 0; i < maxNum; i++)
                {
                    confirmedClients.Add(ClientsInQueue[0]);
                    ClientsInQueue.RemoveAt(0);
                }

                BattleManager.Instance.BeginCreateBattle(confirmedClients);
            }
        }

        public void RemoveFromMatchQueue(TCPClientPeer clientPeer)
        {
            ClientsInQueue.Remove(clientPeer);
        }
    }
}
