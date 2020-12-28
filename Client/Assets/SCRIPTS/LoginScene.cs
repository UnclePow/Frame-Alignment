using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginScene : MonoBehaviour
{
    public GameObject connectPanel;
    public GameObject loginPanel;
    public GameObject input;

    public GameObject reconnectPanel;
    private bool isConnect = false;

    private void Awake()
    {
        NetGlobal.Instance();
        connectPanel.SetActive(true);
        TCPClient.Instance.ConnectServer((res) =>
        {
            if (res == true)
            {
                isConnect = true;
            }
            else
            {
                isConnect = false;
            }

            connectPanel.SetActive(false);
        });
    }

    private void Update()
    {
    }

    public void OnClick_Login()
    {
        if(isConnect == false)
        {
            this.Awake();
        }
        else
        {
            string playerName = input.GetComponent<InputField>().text;
            Login login = new Login { Username = playerName};
            TCPClient.Instance.delegateLoginResponse = LoginResponse;
            TCPClient.Instance.SendMessage<Login>(PB_RequestCode.Login, login);
            loginPanel.SetActive(true);
        }
    }

    private void LoginResponse(LoginResponse loginResponse)
    {
        if(loginResponse.Result == false)
        {
            Debug.Log("登录失败");
            loginPanel.SetActive(false);
        }
        else
        {
            Debug.Log("登录成功,userID:" +loginResponse.Uid);
            loginPanel.SetActive(false);
            NetGlobal.Instance().serverUdpPort = loginResponse.UdpPort;
            NetGlobal.Instance().userID = loginResponse.Uid;

            if (loginResponse.Reconnect == false)
                SceneManager.LoadScene(Config.mainScene);
            else {
                reconnectPanel.SetActive(true);
            } 
        }
    }

    public void OnClick_Reconnect()
    {
        UDPClient.Instance.ConnectServer();

        Reconnect reconnect = new Reconnect {
            Uid = NetGlobal.Instance().userID,
            ClientIP = UDPClient.Instance.localEnd.Address.ToString(),
            ClientUdpPort = UDPClient.Instance.localEnd.Port
        };
        TCPClient.Instance.delegateReconnectResponse = ReconnectResponse;
        TCPClient.Instance.SendMessage<Reconnect>(PB_RequestCode.Reconnect, reconnect);
    }

    public void ReconnectResponse(ReconnectResponse response)
    {
        if(response.Result == false)
        {
            Debug.LogWarning("重连失败----------------");
            return;
        }
        int seed = response.Seed;
        List<BattleUserInfo> players = new List<BattleUserInfo>(response.AllPlayers);
        BattleData.Instance.InitBattleData(seed, players, false, true);
        SceneManager.LoadScene(Config.battleScene);
    }
}
