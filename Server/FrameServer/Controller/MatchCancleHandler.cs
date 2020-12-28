using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer.Controller
{
    class MatchCancleHandler : BaseHandler
    {
        public override void HandlerRequest<T>(T parameters, TCPClientPeer clientPeer)
        {
            MatchCancle matchCancle = parameters as MatchCancle;
            MatchManager.Instance.RemoveFromMatchQueue(clientPeer);
            MatchCancleResponse matchCancleResponse = new MatchCancleResponse();
            clientPeer.SendMessage<MatchCancleResponse>(PB_ResponseCode.MatchCancleResponse, matchCancleResponse);
        }
    }
}
