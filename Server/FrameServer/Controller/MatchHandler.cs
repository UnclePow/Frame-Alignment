using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer.Controller
{
    class MatchHandler : BaseHandler
    {
        public override void HandlerRequest<T>(T parameters, TCPClientPeer clientPeer)
        {
            Match match = parameters as Match;
            string ip = match.ClientIP;
            int port = match.ClientUdpPort;
            clientPeer.clientUpdEnd = new IPEndPoint(IPAddress.Parse(ip), port);
            MatchResponse matchResponse = new MatchResponse { };
            clientPeer.SendMessage<MatchResponse>(PB_ResponseCode.MatchResponse, matchResponse);
            MatchManager.Instance.AddToMatchQueue(clientPeer);
        }
    }
}
