using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider2D_Rect : Collider2D_Base
{
    public int height_con = 100;
    public int width_con  = 100;
    [HideInInspector]
    public int height_logic;
    [HideInInspector]
    public int width_logic;

    public override void InitData()
    {
        height_logic = ToolMethod.Config2Logic(height_con);
        width_logic = ToolMethod.Config2Logic(width_con);
    }

    public override bool IsCollisionCircleCorrection(GameVector2 original_pos_logic, int original_radius_logic, out GameVector2 amend)
    {
        //Debug.LogWarning(width_logic);
        return ToolGameVector.CollideCircleAndRect(original_pos_logic, original_radius_logic, base.baseCenter_logic, width_logic / 2, height_logic / 2, out amend);
        
    }
}
