using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager
{
    public int randomSeed;
    public List<BattleUserInfo> allPlayers;
    public BattleUserInfo localPlayer;
    public Dictionary<int, List<PlayerOperation>> dic_frameData;


    private static ReplayManager _instance;
    public static ReplayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ReplayManager();
            }
            return _instance;
        }
    }

    private ReplayManager(){}

    public void SaveReplay() {
        randomSeed = BattleData.Instance.randSeed;
        allPlayers = BattleData.Instance.battleUsers;
        localPlayer = BattleData.Instance.selfBattleUser;
        dic_frameData = new Dictionary<int, List<PlayerOperation>>();
        int frameID = 0;
        foreach (var item in BattleData.Instance.dic_frameDate) {
            frameID++;
            dic_frameData.Add(frameID, item.Value);
        }
    }
}
