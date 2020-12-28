using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Replay : MonoBehaviour
{
    public Transform map;
    private int frameID = 0;
    private bool isReady = false;
    private bool isFinish = false;
    private float step = 0.066f;
    private float curTime = 0.0f;

    public GameObject finishButton;
    public GameObject replayButton;

    public static Replay instance;    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        finishButton.SetActive(false);
        replayButton.SetActive(false);


        Debug.LogWarning("frame长度：" + ReplayManager.Instance.dic_frameData.Count);
        Debug.LogWarning("玩家长度：" + ReplayManager.Instance.allPlayers.Count);
        map = GameObject.Find("Map").transform;
        gameObject.AddComponent<PlayerManager>();
        gameObject.AddComponent<ObstacleManager>();
        PlayerManager.Instance.Init(map.Find("Player"));
        ObstacleManager.Instance.Init(map.Find("Obstacle"));

        StartCoroutine(WaitInit());
    }

    IEnumerator WaitInit()
    {
        yield return new WaitUntil(() => { return PlayerManager.Instance.isFinish == true && ObstacleManager.Instance.isFinish == true; });
        isReady = true;
        Debug.Log("---------isReady-----------");
    }

    private void Update()
    {
        if (isReady == false || isFinish == true)
            return;
        curTime += Time.deltaTime;
        if (curTime > step)
        {
            frameID++;
            List<PlayerOperation> frameData;
            if(ReplayManager.Instance.dic_frameData.TryGetValue(frameID, out frameData) == false)
            {
                Debug.LogWarning("结束");
                isFinish = true;
                finishButton.SetActive(true);
                replayButton.SetActive(true);
                return;
            }
            PlayerManager.Instance.Logic_Operation(frameData);
            PlayerManager.Instance.Logic_Move();
            curTime = 0.0f;
        }

    }

    public void OnClick_Finish() {
        SceneManager.LoadScene(Config.mainScene);
    }

    public void OnClick_Replay() {
        BattleData.Instance.InitBattleData(ReplayManager.Instance.randomSeed, ReplayManager.Instance.allPlayers, true, false);
        SceneManager.LoadScene(Config.battleScene);
    }
}
