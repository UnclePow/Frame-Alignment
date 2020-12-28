using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameServer.Controller
{
    class LoginHandler : BaseHandler
    {
        public override void HandlerRequest<T>(T parameters, TCPClientPeer clientPeer)
        {
            Login login = parameters as Login;

            bool res = UserManager.Instance.CheckLogin(login.Username);
            if(res == true)
            {
                LoginResponse loginResponse = new LoginResponse
                {
                    Result = false
                };
                clientPeer.SendMessage<LoginResponse>(PB_ResponseCode.LoginResponse, loginResponse);
            }
            else
            {
                int userID = UserManager.Instance.UserLogin(login.Username, clientPeer);
                LoginResponse loginResponse = new LoginResponse
                {
                    Result = true,
                    Uid = userID,
                    UdpPort = ServerConfig.udpPort
                };
                if (UserManager.Instance.CheckReconnect(userID) == true)
                    loginResponse.Reconnect = true;
                else
                    loginResponse.Reconnect = false;

                clientPeer.SendMessage<LoginResponse>(PB_ResponseCode.LoginResponse, loginResponse);
            }
        }
    }
}
