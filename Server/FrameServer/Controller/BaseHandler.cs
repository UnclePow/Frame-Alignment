using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer.Controller
{
    abstract class BaseHandler
    {
        public abstract void HandlerRequest<T>(T parameters, TCPClientPeer clientPeer);
    }
}
