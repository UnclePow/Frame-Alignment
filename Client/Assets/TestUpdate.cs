using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TestUpdate : MonoBehaviour
{

    float t = 0.0f, t1 = 0.0f, t2 = 0.0f;

    //private void Start()
    //{
    //    InvokeRepeating("Repeat", 0, 0.02f);
    //}

    private void FixedUpdate()
    {
        t = Time.realtimeSinceStartup - t;
        Debug.Log(Time.frameCount);
        t = Time.realtimeSinceStartup;

    }

    private void Update()
    {
        t1 = Time.realtimeSinceStartup - t1;
        Debug.LogWarning(Time.frameCount);
        t1 = Time.realtimeSinceStartup;
        
        if (Input.GetMouseButton(1))
        {
            Thread.Sleep(1000);
        }
    }

    private void Repeat()
    {
        t2 = Time.realtimeSinceStartup - t2;
        Debug.Log(t2);
        t2 = Time.realtimeSinceStartup;
    }
}
