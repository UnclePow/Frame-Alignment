using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Collider2D_Base Collider2D_Base;
    public int obstacleID;
    //extend

    public void Init(int id)
    {
        obstacleID = id;
        Collider2D_Base = GetComponent<Collider2D_Base>();
        
        GameVector2 baseCenter_logic = new GameVector2(transform.position.x * ToolMethod.Render2LogicScale, transform.position.z * ToolMethod.Render2LogicScale);

        Collider2D_Base.Init(baseCenter_logic);
    }
}
