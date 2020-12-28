using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;
using Google.Protobuf;

public class TCPClient
{
    public delegate void DelegateOperationResponse<T>(T response);
    public DelegateOperationResponse<LoginResponse> delegateLoginResponse;
    public DelegateOperationResponse<MatchResponse> delegateMatchResponse;
    public DelegateOperationResponse<MatchCancleResponse> delegateMatchCancleResponse;
    public DelegateOperationResponse<BattleEnter> delegateBattleEnterEvent;
    public DelegateOperationResponse<ReconnectResponse> delegateReconnectResponse;

    public Socket clientSocket;
    private Message message;
    private Action<bool> ac_connect;

    private static TCPClient _instance;
    public static TCPClient Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new TCPClient();
            }
            return _instance;
        }
    }

    private TCPClient()
    {

    }

    public void ConnectServer(Action<bool> connect)
    {
        ac_connect = connect;
        message = new Message();
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.BeginConnect(Config.serverIP, Config.serverTcpPort, ConnectCallBack, null);
    }

    private void ConnectCallBack(IAsyncResult asyncResult)
    {

        try
        {
            clientSocket.EndConnect(asyncResult);
            Debug.Log("连接成功");
            NetGlobal.Instance().AddAction(() =>
            {
                ac_connect?.Invoke(true);
            });

            clientSocket.BeginReceive(message.buffer, message.Start, message.Remain, SocketFlags.None, RcvMessageCallBack, null);
        }
        catch (Exception e)
        {
            Debug.LogError("连接错误 "+ e.StackTrace);
            NetGlobal.Instance().AddAction(()=> {
                ac_connect?.Invoke(false);
            });        
        }
    }

    private void RcvMessageCallBack(IAsyncResult asyncResult)
    {
        try
        {
            //Debug.Log("服务器发回来了一条消息");
            int count = clientSocket.EndReceive(asyncResult);
            if (count <= 0)
            {
                Debug.Log("count <= 0");
                clientSocket.Close();
                return;
            }

            message.UnpackMessage(count, HandleResponse);


            clientSocket.BeginReceive(message.buffer, message.Start, message.Remain, SocketFlags.None, RcvMessageCallBack, null);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message+e.StackTrace);
            EndClient();
        }
    }

    public void SendMessage<T>(PB_RequestCode requestCode, T parameter)
    {
        try
        {
            Debug.Log("-->tcp" + requestCode.ToString() + ":" + parameter.ToString());
            byte[] data = Message.PackMessage<T>(requestCode, parameter);
            clientSocket.Send(data);            
        }
        catch (Exception e)
        {
            Debug.LogWarning("tcp发送数据异常"+e.StackTrace);
            EndClient();
        }
    }

    public void EndClient()
    {
        if (clientSocket != null)
        {
            try
            {
                clientSocket.Close();
                clientSocket = null;
                Debug.Log("关闭tcp连接--------------");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("关闭tcp连接异常:" + ex.Message);
            }
        }
    }

    private void HandleResponse(byte responseByte, byte[] responseParams)
    {
        PB_ResponseCode responseCode = (PB_ResponseCode)responseByte;
        Debug.Log("<--tcp" + responseCode.ToString());
        switch (responseCode)
        {
            case PB_ResponseCode.LoginResponse: {
                    LoginResponse loginResponse = Message.Deserialize<LoginResponse>(responseParams);
                    if(delegateLoginResponse != null)
                    {
                        NetGlobal.Instance().AddAction(() =>
                        {
                            delegateLoginResponse(loginResponse);
                        });
                    }
                    break;
                }
            case PB_ResponseCode.MatchResponse: {
                    MatchResponse matchResponse = Message.Deserialize<MatchResponse>(responseParams);
                    if(delegateMatchResponse != null)
                    {
                        NetGlobal.Instance().AddAction(() => { delegateMatchResponse(matchResponse); });
                    }
                    break;
                }

            case PB_ResponseCode.MatchCancleResponse: {
                    MatchCancleResponse matchCancleResponse = Message.Deserialize<MatchCancleResponse>(responseParams);
                    if(delegateMatchCancleResponse != null)
                    {
                        NetGlobal.Instance().AddAction(() =>
                        {
                            delegateMatchCancleResponse(matchCancleResponse);
                        });
                    }
                    break;
                }

            case PB_ResponseCode.BattleEnter: {
                    BattleEnter battleEnter = Message.Deserialize<BattleEnter>(responseParams);
                    if(delegateBattleEnterEvent != null)
                    {
                        NetGlobal.Instance().AddAction(()=> {
                            delegateBattleEnterEvent(battleEnter);
                        });
                    }
                    break;
                }

            case PB_ResponseCode.ReconnectResponse: {
                    ReconnectResponse reconnectResponse = Message.Deserialize<ReconnectResponse>(responseParams);
                    if(delegateReconnectResponse != null)
                    {
                        NetGlobal.Instance().AddAction(() => {
                            delegateReconnectResponse(reconnectResponse);
                        });
                    }
                    break;
                }
        }
    }
}
