using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchScene : MonoBehaviour
{
    public GameObject matchingPanel;
    private bool isMatching = false;
    private void Start()
    {
        matchingPanel.SetActive(false);
    }

    public void OnClick_Match()
    {
        if(isMatching == false)
        {
            MatchRequest();
        }
        else
        {
            CancelMatch();
        }
    }

    private void MatchRequest()
    {
        UDPClient.Instance.ConnectServer();
        Match match = new Match
        {
            Uid = NetGlobal.Instance().userID,
            ClientIP = UDPClient.Instance.localEnd.Address.ToString(),
            ClientUdpPort = UDPClient.Instance.localEnd.Port
        };
        TCPClient.Instance.delegateMatchResponse = MatchResponse;
        TCPClient.Instance.delegateBattleEnterEvent = BattleEnterEvent;
        TCPClient.Instance.SendMessage<Match>(PB_RequestCode.Match, match);
    }

    private void CancelMatch()
    {
        MatchCancle matchCancle = new MatchCancle
        {
            Uid = NetGlobal.Instance().userID,
        };
        TCPClient.Instance.delegateMatchCancleResponse = CancelMatchResponse;        
        TCPClient.Instance.SendMessage<MatchCancle>(PB_RequestCode.MatchCancle, matchCancle);
    }

    private void MatchResponse(MatchResponse matchResponse)
    {
        isMatching = true;
        matchingPanel.SetActive(true);
    }

    private void CancelMatchResponse(MatchCancleResponse matchCancleResponse)
    {
        isMatching = false;
        matchingPanel.SetActive(false);
    }

    private void BattleEnterEvent(BattleEnter battleEnter)
    {
        //Debug.LogWarning("执行了");
        int seed = battleEnter.Seed;
        List<BattleUserInfo> battleUsers = new List<BattleUserInfo>(battleEnter.AllPlayers);
        BattleData.Instance.InitBattleData(seed, battleUsers, false, false);
        SceneManager.LoadScene(Config.battleScene);
    }
}
