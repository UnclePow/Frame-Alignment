using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

public class NetGlobal
{
    private List<Action> action_list = new List<Action>();
    private Mutex mutex_action_list = new Mutex();

    public int userID;
    public int serverUdpPort;

    private static readonly object _lock = new object();
    private static NetGlobal _instance;
    public static NetGlobal Instance()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = new NetGlobal();
            }
            return _instance;
        }

    }

    private NetGlobal()
    {
        GameObject go = new GameObject("Update_MainThread");
        go.AddComponent<NetUpdate>();
        GameObject.DontDestroyOnLoad(go);
    }

    public void AddAction(Action action)
    {
        mutex_action_list.WaitOne();
        action_list.Add(action);
        mutex_action_list.ReleaseMutex();
    }

    public void DoAction()
    {
        mutex_action_list.WaitOne();
        foreach(var action in action_list)
        {
            try
            {
                action();
            }catch (Exception e)
            {
                Debug.LogWarning(e.Message + e.StackTrace);
            }
        }
        action_list.Clear();
        mutex_action_list.ReleaseMutex();
    }
}

public class NetUpdate: MonoBehaviour
{
    private void Update()
    {
        NetGlobal.Instance().DoAction();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("退出");
        TCPClient.Instance.EndClient();
        UDPClient.Instance.EndClient();
    }
}
