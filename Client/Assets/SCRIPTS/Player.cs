using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int moveSpeed = 10;
    public int attackTime;//攻击时间间隔
    private int curAttackTime;
    //	private GameObject uiObj;
    private Transform modleParent;

    private GameVector2 logicPosition;
    private Vector3 renderPosition;  // 渲染位置
    private Quaternion renderDir;
    private GameVector2 logicSpeed;
    
    private int roleDirection;//角色朝向

    private Animator animator;
    public Collider2D_Base collider2D_Base;

    public void Init(GameVector2 logicPos)
    {
        logicPosition = logicPos;
        renderPosition = ToolGameVector.ChangeGameVectorToVector3(logicPosition);
        renderDir = transform.rotation;
        logicSpeed = GameVector2.zero;
        roleDirection = 0;

        animator = GetComponentInChildren<Animator>();
        collider2D_Base = GetComponent<Collider2D_Base>();
        collider2D_Base.Init(logicPosition);
    }

    void Update()
    {
        if (BattleData.Instance.isRender)
        {
            transform.position = Vector3.Lerp(transform.position, renderPosition, 0.4f);
            transform.rotation = Quaternion.Lerp(transform.rotation, renderDir, 0.2f);
        }
    }

    //获取帧操作对应的一帧内的速度
    public virtual void Logic_UpdateMoveDir(int _dir)
    {
        if (_dir > 120)
        {
            animator.SetFloat("Forward", 0.0f);
            logicSpeed = GameVector2.zero;
        }
        else
        {
            animator.SetFloat("Forward", 1.0f);
            roleDirection = _dir * 3;
            logicSpeed = moveSpeed * BattleData.Instance.GetSpeed(roleDirection);
            Vector3 _renderDir = ToolGameVector.ChangeGameVectorToVector3(logicSpeed);
            renderDir = Quaternion.LookRotation(_renderDir);
        }
    }



    public virtual void Logic_Move()
    {

        //  Debug.Log("Logic_Move  "  + Time.realtimeSinceStartup);
        if (logicSpeed != GameVector2.zero)
        { // 如果逻辑速度不等于0
            //GameVector2 _targetPos = objShape.GetPosition() + logicSpeed; // 计算目标位置            
            //UpdateLogicPosition(_targetPos); //更新逻辑位置， 
            //renderPosition = objShape.GetPositionVec3(); // 更新渲染位置。 使用算法平滑处理。
            GameVector2 targetLogicPos = logicPosition + logicSpeed;
            logicPosition = targetLogicPos;
            collider2D_Base.baseCenter_logic = logicPosition;
        }
        
        ObstacleManager.Instance.CollideAndAdjust(collider2D_Base.baseCenter_logic, collider2D_Base.baseRadius_logic, out GameVector2 correct_logic_pos);
        collider2D_Base.baseCenter_logic = correct_logic_pos;
        logicPosition = collider2D_Base.baseCenter_logic;

        renderPosition = ToolGameVector.ChangeGameVectorToVector3(logicPosition);
        //if (curAttackTime > 0)
        //{
        //    curAttackTime--;
        //}
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }

    //public virtual void Logic_Move_Correction()
    //{
    //    GameVector2 _ccLogicPos;
    //    if (BattleCon.Instance.obstacleManage.CollisionCorrection(objShape.GetPosition(), objShape.GetRadius(), out _ccLogicPos))
    //    {
    //        UpdateLogicPosition(_ccLogicPos);
    //    }
    //}

    //void UpdateLogicPosition(GameVector2 _logicPos)
    //{
    //    objShape.SetPosition(BattleData.Instance.GetMapLogicPosition(_logicPos));
    //}
}
