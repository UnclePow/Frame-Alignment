using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager:MonoBehaviour
{
    public bool isFinish;
    public Transform playerParent;
    public GameObject playerPrefab;
    Dictionary<int, Player> dic_players;//uid - Player

    private static PlayerManager _instance;
    public static PlayerManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private void Awake()
    {
        _instance = this;
    }

    public void Init(Transform playerParent)
    {
        isFinish = false;
        this.playerParent = playerParent;
        dic_players = new Dictionary<int, Player>();
        playerPrefab = Resources.Load<GameObject>("PlayerBase");
        StartCoroutine(InstantiatePlayers());
    }

    IEnumerator InstantiatePlayers()
    {
        List<BattleUserInfo> list_battleUsers = BattleData.Instance.battleUsers;
        for (int i = 0; i < list_battleUsers.Count; i++) {
            yield return new WaitForEndOfFrame();
            BattleUserInfo battleUser = list_battleUsers[i];

            GameObject player = Instantiate(playerPrefab, playerParent);
            GameVector2 logicPosition = new GameVector2(ToolMethod.Render2Logic(1 * i), ToolMethod.Render2Logic(0));
            player.GetComponent<Player>().Init(logicPosition);
            dic_players.Add(battleUser.Uid, player.GetComponent<Player>());
        }

        isFinish = true;
    }

    public void Logic_Operation(List<PlayerOperation> frameData)
    {
        foreach (PlayerOperation item in frameData)
        {
            dic_players[item.BattleUserInfo.Uid].Logic_UpdateMoveDir(item.Operation.Move);            
        }
    }

    public void Logic_Move()
    { // 逻辑移动。遍历每一个角色完成移动
      // Debug.Log("dic_role.Count  "  + dic_role.Count);
        foreach (var item in dic_players)
        {
            item.Value.Logic_Move();
        }
    }

    //public void Logic_Move_Correction()
    //{
    //    foreach (var item in dic_players)
    //    {
    //        item.Value.Logic_Move_Correction();
    //    }
    //}
}
