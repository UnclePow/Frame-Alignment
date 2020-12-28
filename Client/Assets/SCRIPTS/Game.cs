using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject battlePanel;
    public GameObject replayPanel;

    private void Awake()
    {
        if (BattleData.Instance.isReplay == true)
        {
            GetComponent<Replay>().enabled = true;
            battlePanel.SetActive(false);
            replayPanel.SetActive(true);
        }
        else if(BattleData.Instance.isReconnect == true)
        {
            GetComponent<BattleReconnect>().enabled = true;
            battlePanel.SetActive(true);
            replayPanel.SetActive(false);
        }
        else
        {
            GetComponent<Battle>().enabled = true;
            battlePanel.SetActive(true);
            replayPanel.SetActive(false);
        }
    }
}
