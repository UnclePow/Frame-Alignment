using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 1.等待资源加载完毕---
/// 2.发送BattleReady(loop)
/// 3.等待接受BattleStart---
/// 4.发送帧数据(loop 66ms)
/// 5.等待接受服务器下发的第一帧---
/// 6.开始逻辑同步
/// </summary>
public class Battle : MonoBehaviour
{
    protected bool isBattleStart = false;
    public GameObject loadingPanel;
    public GameObject finishLoadButton;
    public GameObject gameOverButton;
    public GameObject gameoverPanel;
    public Transform map;

    public static Battle instance;
    private void Awake()
    {
        instance = this;
    }

    protected virtual void Start()
    {
        loadingPanel.SetActive(true);
        loadingPanel.GetComponentInChildren<Text>().text = "加载资源中....";
        finishLoadButton.SetActive(false);
        gameOverButton.SetActive(false);
        gameoverPanel.SetActive(false);

        //UDPClient.Instance.ConnectServer();
        UDPClient.Instance.delegateBattleStart = Rcv_BattleStart;
        UDPClient.Instance.delegateDownFrameData = Rcv_DataFrame;
        UDPClient.Instance.delegateResponseLackFrames = Rcv_LackFrames;
        UDPClient.Instance.delegateGameOver = Rcv_GameOver;

        Transform playerParent = map.Find("Player");
        Transform obstacleParent = map.Find("Obstacle");
        gameObject.AddComponent<PlayerManager>();        
        gameObject.AddComponent<ObstacleManager>();
        PlayerManager.Instance.Init(playerParent);
        ObstacleManager.Instance.Init(obstacleParent);

        StartCoroutine(WaitInitData());
    }

    IEnumerator WaitInitData()
    {
        yield return new WaitUntil(() => { return PlayerManager.Instance.isFinish == true && ObstacleManager.Instance.isFinish == true; });
        //InvokeRepeating("Send_BattleReady", 0, 0.2f);
        finishLoadButton.SetActive(true);
    }

    

    protected virtual IEnumerator WaitForFirstMessage()
    {
        yield return new WaitUntil(() => { return BattleData.Instance.dic_frameDate.Count > 0; });
        //可以进行逻辑同步了
        Debug.LogWarning("-----------开始逻辑更新--------------");
        InvokeRepeating("LogicUpdate", 0, 0.02f);

        loadingPanel.SetActive(false);
        gameOverButton.SetActive(true);
        finishLoadButton.SetActive(false);
    }

    /// <summary>
    /// 服务器每66ms下发一帧，而LogicUpdate每隔20ms执行一次
    /// 因此并不是每一次执行LogicUpdate都能拿到最新的一帧，所以需要进行判断
    /// 如果没有新帧的话就不需要进行处理了
    /// </summary>
    protected virtual void LogicUpdate()
    {        
        List<PlayerOperation> frameData;
        if (BattleData.Instance.TryGetLastestFrame(out frameData) == true) {
            //为true说明服务器又发来了新帧
            //TODO
            //foreach(var item in frameData)
            //{
            //    Debug.Log(item.ToString());
            //}
            PlayerManager.Instance.Logic_Operation(frameData);
            PlayerManager.Instance.Logic_Move();

            BattleData.Instance.curFramID++;
        }
    }



    public virtual void Send_BattleReady()
    {
        BattleReady battleReady = new BattleReady { BattleUserInfo = BattleData.Instance.selfBattleUser };
        //发送Battle Ready后期待服务器返回BattleStart        
        UDPClient.Instance.SendMessage<BattleReady>(PB_RequestCode.BattleReady, battleReady);

        loadingPanel.GetComponentInChildren<Text>().text = "等待其它玩家加载...";
    }

    protected virtual void Send_PlayerOperation()
    {
        BattleData.Instance.mesNum++;
        UpPlayerOperation upPlayerOperation = new UpPlayerOperation
        {
            PlayerOperation = BattleData.Instance.selfOperation,
            MesID = BattleData.Instance.mesNum            
        };
        UDPClient.Instance.SendMessage<UpPlayerOperation>(PB_RequestCode.UpPlayerOperation, upPlayerOperation);
    }

    public void Send_GameOver()
    {
        RequestGameOver requestGameOver = new RequestGameOver
        {
            BattleUserInfo = BattleData.Instance.selfBattleUser
        };
        UDPClient.Instance.SendMessage<RequestGameOver>(PB_RequestCode.RequestGameOver, requestGameOver);
    }


    public void Rcv_BattleStart(BattleStart battleStart)
    {
        if (isBattleStart == true)
        {
            Debug.Log("收到BattleStart，帧数据已开始发送-----------");
            return;
        }
        Debug.Log("收到BattleStart，开始发送帧数据-----------");
        isBattleStart = true;
        CancelInvoke("Send_BattleReady");
        float _time = Config.frameTime * 0.001f;  // 66ms
        this.InvokeRepeating("Send_PlayerOperation", _time, _time);  // 循环调用 Send_operation 方法

        StartCoroutine(WaitForFirstMessage());
    }

    protected virtual void Rcv_DataFrame(DownPlayerOperations frame)
    {
        int frameID = frame.FrameID;
        List<PlayerOperation> frameData = new List<PlayerOperation>(frame.PlayerOperations);
        BattleData.Instance.AddNewFrame(frameID, frameData);
    }

    protected virtual void Rcv_LackFrames(ResponseLackFrames responseLackFrames)
    {
        foreach (DownPlayerOperations lackFrame in responseLackFrames.LackFrames)
        {
            int frameID = lackFrame.FrameID;
            List<PlayerOperation> frameData = new List<PlayerOperation>(lackFrame.PlayerOperations);
            BattleData.Instance.AddLackFrames(frameID, frameData);
        }
    }
    public void Rcv_GameOver(ResponseGameOver responseGameOver)
    {
        CancelInvoke("Send_GameOver");
        CancelInvoke("Send_PlayerOperation");
        gameoverPanel.SetActive(true);
        gameOverButton.SetActive(false);

    }

    public void ToMainScene()
    {
        //BattleData.Instance.InitBattleData(0, null, false);
        SceneManager.LoadScene(Config.mainScene);
    }

    private bool flag = false;
    public void OnClick_Replay()
    {
        if (flag) {
            BattleData.Instance.InitBattleData(ReplayManager.Instance.randomSeed, ReplayManager.Instance.allPlayers, true, false);
            SceneManager.LoadScene(Config.battleScene);
            //Debug.Log($"帧长度{ReplayManager.Instance.dic_frameData.Count}");
            return;
        }
        ReplayManager.Instance.SaveReplay();
       // Debug.Log($"帧长度{ReplayManager.Instance.dic_frameData.Count}");
        flag = true;
        GameObject.Find("Record").GetComponentInChildren<Text>().text = "查看录像";
    }
}
