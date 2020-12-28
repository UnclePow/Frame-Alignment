using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private static ObstacleManager instance;
    public static ObstacleManager Instance
    {
        get
        {
            return instance;
        }
    }
    private void Awake()
    {
        instance = this;
    }

    public bool isFinish;
    private Transform obstacleParent;

    public Dictionary<int, Obstacle> dic_obstacle;
    private int curID = 0;
    

    public void Init(Transform obstacleP)
    {
        isFinish = false;
        this.obstacleParent = obstacleP;
        dic_obstacle = new Dictionary<int, Obstacle>();
        //将场景中的obstacle加入字典中，统一管理
        Obstacle[] obstaclesInScene = GameObject.FindObjectsOfType<Obstacle>();
        for (int i = 0; i < obstaclesInScene.Length; i++) {
            curID++;
            dic_obstacle.Add(curID, obstaclesInScene[i]);
            obstaclesInScene[i].transform.SetParent(obstacleParent);
            obstaclesInScene[i].Init(curID);
        }
        isFinish = true;
    }

    /// <summary>
    /// 判断一个物体是否与obstacles（dic_obstacles中的所有obstacles）发生碰撞，是的话返回修正之后的位置
    /// </summary>
    /// <param name="logic_center">logic pos</param>
    /// <param name="logic_radius">logic radius</param>
    /// <param name="correct_logic_center"></param>
    /// <returns>是否与obstac发生碰撞</returns>
    public bool CollideAndAdjust(GameVector2 logic_center, int logic_radius, out GameVector2 correct_logic_center)
    {
        //为了优化计算，对障碍物的碰撞体进行分层（两层），顶层为圆形碰撞体（Collider2D_Base）
        //下层为矩形碰撞体Collider2D_Rect(前提是该障碍物碰撞体形状为矩形)
        bool result = false;
        correct_logic_center = logic_center;
        foreach(var obstacle in dic_obstacle.Values) { 
            GameVector2 logic_center2 = obstacle.Collider2D_Base.baseCenter_logic;
            int logic_radius2 = obstacle.Collider2D_Base.baseRadius_logic;
            if (ToolGameVector.CollideCircleAndCircle(logic_center, logic_center2, logic_radius, logic_radius2) == false) {
                //如果玩家的碰撞体（圆形）连障碍物的外层球型包围盒都没有碰到的话，直接退出，检测与下一个障碍物的碰撞
                continue;
            }
            //Debug.LogWarning(obstacle.obstacleID + " " + obstacle.Collider2D_Base.baseCenter_logic);
            GameVector2 amend;
            if (obstacle.Collider2D_Base.IsCollisionCircleCorrection(logic_center, logic_radius, out amend) == true)
            {
                correct_logic_center += amend;
                result = true;
                //Debug.LogWarning(obstacle.obstacleID +"---------------"+ obstacle.Collider2D_Base.baseCenter_logic);
            }

        }
        return result;
    }
}
