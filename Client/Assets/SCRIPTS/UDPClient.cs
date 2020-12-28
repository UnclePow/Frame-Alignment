using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;

public class UDPClient
{
    private static UDPClient _instance;
    public static UDPClient Instance {
        get
        {
            if(_instance == null)
            {
                _instance = new UDPClient();
            }
            return _instance;
        }
    }
    public delegate void DelegateOperationResonse<T>(T udpResponse);
    public DelegateOperationResonse<BattleStart> delegateBattleStart;
    public DelegateOperationResonse<DownPlayerOperations> delegateDownFrameData;
    public DelegateOperationResonse<ResponseLackFrames> delegateResponseLackFrames;
    public DelegateOperationResonse<ResponseGameOver> delegateGameOver;
    public UdpClient udpClient;
    public IPEndPoint localEnd;
    private bool isRun = false;

    private UDPClient() {
        //ConnectServer();
    }

    public void ConnectServer()
    {
        try
        {
            if (isRun == true)
                return;

            udpClient = new UdpClient();
            udpClient.Connect(Config.serverIP, NetGlobal.Instance().serverUdpPort);//可能会抛出exception
            localEnd = udpClient.Client.LocalEndPoint as IPEndPoint;
            isRun = true;

            Thread rcvThread = new Thread(new ThreadStart(RcvMessage));
            rcvThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
        }
    }

    public void RcvMessage()
    {
        try
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (isRun)
            {
                byte[] buffer = udpClient.Receive(ref iPEndPoint);

                if (buffer.Length <= 4)
                {
                    throw new Exception("接受的udp数据长度小于4");
                }
                int count = BitConverter.ToInt32(buffer, 0);
                byte responseCode = buffer[4];
                byte[] parameters = new byte[count - 1];
                Array.Copy(buffer, 5, parameters, 0, count - 1);

                this.AnalyzeMessage(responseCode, parameters);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message + e.StackTrace);
        }
    }

    private void AnalyzeMessage(byte responseCode, byte[] parameters)
    {        
        try
        {
            PB_ResponseCode response = (PB_ResponseCode)responseCode;
            Debug.Log("<--udp" + response.ToString());
            switch (response)
            {
                case PB_ResponseCode.BattleStart:
                    {
                        BattleStart battleStart = Message.Deserialize<BattleStart>(parameters);
                        if (delegateBattleStart != null)
                        {
                            NetGlobal.Instance().AddAction(() =>
                            {
                                delegateBattleStart(battleStart);
                            });
                        }
                        else
                            throw new Exception("delegateBattleStart is null");
                        break;
                    }
                case PB_ResponseCode.DownPlayerOperations:
                    {
                        DownPlayerOperations downFramedata = Message.Deserialize<DownPlayerOperations>(parameters);
                        if (delegateDownFrameData != null)
                        {
                            NetGlobal.Instance().AddAction(() =>
                            {
                                delegateDownFrameData(downFramedata);
                            });
                        }
                        else
                            throw new Exception("delegateDownFrameData is null");
                        break;
                    }

                case PB_ResponseCode.ResponseLackFrames: {
                        ResponseLackFrames responseLackFrames = Message.Deserialize<ResponseLackFrames>(parameters);
                        if (delegateResponseLackFrames != null)
                        {
                            NetGlobal.Instance().AddAction(() =>
                            {
                                delegateResponseLackFrames(responseLackFrames);
                            });
                        }
                        else
                            throw new Exception("delegateResponseLackFrames is null");
                        break;
                    }

                case PB_ResponseCode.ResponseGameOver: {
                        ResponseGameOver responseGameOver = Message.Deserialize<ResponseGameOver>(parameters);
                        if (delegateGameOver != null)
                        {
                            NetGlobal.Instance().AddAction(() =>
                            {
                                delegateGameOver(responseGameOver);
                            });
                        }
                        else
                            throw new Exception("delegateGameOver is null");
                        break;
                    }
            }
        }
        catch (Exception e)
        {//一般情况下是不会出现异常的-----
            Debug.LogWarning(e.Message + e.StackTrace);
        }
    }

    public void SendMessage<T>(PB_RequestCode requestCode, T parameters)
    {
        try
        {
            Debug.Log("-->udp" + requestCode.ToString() +":"+parameters.ToString());
            if (isRun == false)
                throw new Exception("与服务器断开udp连接，无法发送数据~");
            byte[] data = Message.PackMessage<T>(requestCode, parameters);
            udpClient.Send(data, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message + e.StackTrace);           
        }
    }

    public void EndClient()
    {
        isRun = false;
        if(udpClient != null)
        {
            try
            {
                udpClient.Close();
                udpClient = null;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
