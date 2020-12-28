using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer
{
    class ServerConfig
    {
        public static string serverIP = "127.0.0.1";
        public static int tcpPort = 30001;
        public static int udpPort = 31001;

        //66ms
        public static int frameTime = 66;
    }
}
