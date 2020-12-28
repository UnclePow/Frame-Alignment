using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer
{
    class StartApplication
    {
        public static void Main(string[] args)
        {
            TCPServer.Instance.StartServer();
            UDPServer.Instance.StartServer();
            Console.ReadLine();
 
        }
    }
}
