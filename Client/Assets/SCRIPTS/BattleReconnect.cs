using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleReconnect : Battle
{
    protected override void Start()
    {
        base.Start();
        BattleData.Instance.isRender = false;
        UDPClient.Instance.delegateBattleStart = null;
        UDPClient.Instance.delegateDownFrameData = null;
        UDPClient.Instance.delegateResponseLackFrames = null;
        finishLoadButton.GetComponent<Button>().onClick.RemoveAllListeners();
        finishLoadButton.GetComponent<Button>().onClick.AddListener(OnClick_LoadFinish);
    }

    public void OnClick_LoadFinish()
    {
        this.Send_BattleReady();
    }

    public override void Send_BattleReady()
    {
        //开始接受帧数据
        UDPClient.Instance.delegateDownFrameData = Rcv_DataFrame;
    }

    private bool flag = true;
    private float chaseSpeed = 0;
    protected override void Rcv_DataFrame(DownPlayerOperations frame)
    {
        int frameID = frame.FrameID;
        List<PlayerOperation> frameData = new List<PlayerOperation>(frame.PlayerOperations);
        //第一次收到的话必会检测到丢帧
        //随后请求丢失的帧
        BattleData.Instance.AddNewFrame(frameID, frameData);

        if (flag)
        {
            //第一次收到服务器下发的帧数据
            int lackNum = frameID - 1;
            chaseSpeed = 5 / lackNum;
            
            //BattleStart();
            UDPClient.Instance.delegateResponseLackFrames = Rcv_LackFrames;
            flag = false;

            Debug.LogError("第一次收到服务器下发的帧，一共丢失的帧数：" + lackNum);
        }
    }

    private bool flag1 = true;
    protected override void Rcv_LackFrames(ResponseLackFrames responseLackFrames)
    {
        foreach (DownPlayerOperations lackFrame in responseLackFrames.LackFrames)
        {
            int frameID = lackFrame.FrameID;
            List<PlayerOperation> frameData = new List<PlayerOperation>(lackFrame.PlayerOperations);
            BattleData.Instance.AddLackFrames(frameID, frameData);
        }

        if (flag1)
        {//第一次收到传回的所有丢失的帧
            
            BattleStart();
            flag1 = false;

            Debug.LogError("第一次收到传回的所有丢失的帧--------");
        }
    }

    private void BattleStart()
    {
        isBattleStart = true;
        CancelInvoke("Send_BattleReady");
        float _time = Config.frameTime * 0.001f;  // 66ms
        InvokeRepeating("Send_PlayerOperation", _time, _time);  // 循环调用 Send_operation 方法

        Debug.LogWarning("-----------开始逻辑更新--------------");
        InvokeRepeating("LogicUpdate", 0, 0.0001f);
        //Debug.LogError(chaseSpeed);
        loadingPanel.SetActive(false);
        gameOverButton.SetActive(true);
        finishLoadButton.SetActive(false);
    }


    private bool flag2 = true;
    protected override void LogicUpdate()
    {
        List<PlayerOperation> frameData;
        if (BattleData.Instance.TryGetLastestFrame(out frameData) == true)
        {
            PlayerManager.Instance.Logic_Operation(frameData);
            PlayerManager.Instance.Logic_Move();

            BattleData.Instance.curFramID++;
        }
        else
        {
            if (flag2)
            {//追赶上了
                BattleData.Instance.isRender = true;
                CancelInvoke("LogicUpdate");
                InvokeRepeating("LogicUpdate", 0, 0.02f);
                flag2 = false;
                Debug.LogError("追赶上了----------");
            }
        }
    }
}
