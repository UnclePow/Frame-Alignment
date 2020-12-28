using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrameServer
{
    class UDPClientPeer
    {
        public int uid;
        public IPEndPoint clientEndPoint;//关键
        public UdpClient udpClient;//公共的
        

        public UDPClientPeer(int uid, IPEndPoint clientEnd)
        {
            this.uid = uid;
            this.clientEndPoint = clientEnd;
            udpClient = UDPServer.Instance.GetUdpClient();
        }

        public void SendMessage<T>(PB_ResponseCode responseCode, T parameters)
        {
            try
            {
                //Console.WriteLine("-->tcp:" + clientEndPoint.ToString() + ":" + responseCode.ToString() + ":" + parameters.ToString());
                byte[] data = Message.PackMessage<T>(responseCode, parameters);
                udpClient.Send(data, data.Length, clientEndPoint);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
