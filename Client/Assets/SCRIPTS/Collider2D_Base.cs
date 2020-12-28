using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2D_Base : MonoBehaviour
{
    [HideInInspector]
    public GameVector2 baseCenter_logic;
    
    public int baseRadius_con = 50;
    [HideInInspector]
    public int baseRadius_logic;

    public void Init(GameVector2 logic_pos)
    {
        baseCenter_logic = logic_pos;
        baseRadius_logic = ToolMethod.Config2Logic(baseRadius_con);

        InitData();
    }

    public virtual void InitData()
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="original_pos_logic"></param>
    /// <param name="original_radius_logic"></param>
    /// <param name="amend"></param>
    /// <returns></returns>
    public virtual bool IsCollisionCircleCorrection(GameVector2 original_pos_logic, int original_radius_logic, out GameVector2 amend)
    {
        return ToolGameVector.CollideCircleAndCircle(original_pos_logic, baseCenter_logic, original_radius_logic, baseRadius_logic, out amend);   // 角色位置，  障碍位置， 角色半径，障碍半径。
    }
}
