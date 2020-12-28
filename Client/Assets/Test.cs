using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Test : MonoBehaviour
{
    public GameVector2 circleCenter;
    public int circleRadius;
    public GameVector2 rectCenter;
    public int halfHeight;
    public int halfWidth;
    public GameVector2 amend;
    public bool res;
    void Update()
    {
        //res = ToolGameVector.CollideCircleAndRect(circleCenter, circleRadius, rectCenter, halfHeight, halfWidth, out amend);
    }
}
