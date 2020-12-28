using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleData
{
    #region SingleInstance
    private static BattleData _instance;
    public static BattleData Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new BattleData();
            }
            return _instance;
        }
    }
    #endregion
    public bool isReplay;
    public bool isReconnect;
    public bool isRender = true;

    public int randSeed;
    public List<BattleUserInfo> battleUsers;
    public int battleID;
    public BattleUserInfo selfBattleUser;

    //private int curOperationID;
    public int mesNum;//发送的帧总量
    public PlayerOperation selfOperation;//本地玩家的帧操作


    public int curFramID;//当前帧号
    public int maxFrameID;//用于缺帧相关

    public Dictionary<int, List<PlayerOperation>> dic_frameDate;//储存接受到的所有帧
    public Dictionary<int, GameVector2> dic_speed;
    private BattleData() {
        
        dic_speed = new Dictionary<int, GameVector2>();
        //初始化速度表
        GlobalData.Instance().GetFileStringFromStreamingAssets("Desktopspeed.txt", _fileStr => {
            InitSpeedInfo(_fileStr);
        });        
    }

    public void InitBattleData(int seed, List<BattleUserInfo> battleUsers, bool isReplay, bool isReconnect)
    {
        //curOperationID = 1;
        mesNum = 0;
        selfOperation = new PlayerOperation();
        selfOperation.Operation = new Operation();
        selfOperation.Operation.Move = 121;

        curFramID = 0;
        maxFrameID = 0;
        dic_frameDate = new Dictionary<int, List<PlayerOperation>>();
        lackFrameIDs = new List<int>();

        this.randSeed = seed;
        this.battleUsers = new List<BattleUserInfo>(battleUsers);
        this.battleID = battleUsers[0].BattleID;
        this.isReplay = isReplay;
        this.isReconnect = isReconnect;
        foreach(var item in battleUsers)
        {
            if(item.Uid == NetGlobal.Instance().userID)
            {
                this.selfBattleUser = new BattleUserInfo(item);
                selfOperation.BattleUserInfo = this.selfBattleUser;
                break;
            }
        }

        
    }


    private List<int> lackFrameIDs;
    public void AddNewFrame(int frameID, List<PlayerOperation> frameData)
    {
        dic_frameDate[frameID] = frameData;
        //Debug.LogWarning(dic_frameDate.Count);
        for (int i = maxFrameID + 1; i < frameID; i++)
        {
            lackFrameIDs.Add(i);
            //Debug.LogWarning("----------------丢帧，帧号：" + i);
        }
        maxFrameID = frameID;
        if(lackFrameIDs.Count > 0)
        {
            Debug.LogWarning("----------------丢帧，总帧数：" + lackFrameIDs.Count);
            RequestLackFrames requestLackFrames = new RequestLackFrames();
            requestLackFrames.BattleUserInfo = selfBattleUser;
            requestLackFrames.LackFrameIDs.AddRange(lackFrameIDs);
            UDPClient.Instance.SendMessage<RequestLackFrames>(PB_RequestCode.RequestLackFrames, requestLackFrames);
        }
    }

    public void AddLackFrames(int frameID, List<PlayerOperation> frameData)
    {
        if(lackFrameIDs.Contains(frameID) == true)
        {
            dic_frameDate[frameID] = frameData;
            lackFrameIDs.Remove(frameID);
        }
    }

    public bool TryGetLastestFrame(out List<PlayerOperation> frameData)
    {
        int frameID = curFramID + 1;
        return dic_frameDate.TryGetValue(frameID, out frameData);
    }

    public void UpdateMoveDir(int _dir)
    {
        // Debug.Log("_dir  ************   "  + _dir);
        selfOperation.Operation.Move = _dir;
    }

    private void InitSpeedInfo(string _fileStr)
    {
        string[] lineArray = _fileStr.Split("\n"[0]);

        int dir;
        for (int i = 0; i < lineArray.Length; i++)
        {
            if (lineArray[i] != "")
            {
                GameVector2 date = new GameVector2();
                string[] line = lineArray[i].Split(new char[1] { ',' }, 3);
                dir = System.Int32.Parse(line[0]);
                date.x = System.Int32.Parse(line[1]);
                date.y = System.Int32.Parse(line[2]);
                dic_speed[dir] = date;
            }
        }
    }

    public GameVector2 GetSpeed(int dir)
    {
        GameVector2 speed = GameVector2.zero;
        try
        {
            speed = dic_speed[dir];
        }
        catch (System.Exception e)
        {
            Debug.Log("----------------------------------------------------------");
            Debug.Log($"-----------dic_speed.count={dic_speed.Count}--------------");
            Debug.Log("----------------------------------------------------------");
        }
        speed = dic_speed[dir];        
        return speed;
    }
}
