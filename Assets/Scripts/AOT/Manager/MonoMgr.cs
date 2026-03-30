using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// 公共的mono管理器  其他没有继承monobehavivor的类 也可以使用协程 还有调用update方法  
/// </summary>
public class MonoMgr : UnitySingleTonMono<MonoMgr>
{
    private event UnityAction updateEvent;

    void Start()
    {
    }

    /// <summary>
    /// 添加监听事件函数  
    /// </summary>
    /// <param name="a"></param>
    public void addUpdateListener(UnityAction a)
    {
        updateEvent += a;
    }

    /// <summary>
    /// 移除事件函数
    /// </summary>
    /// <param name="a"></param>
    public void removeUpdateListener(UnityAction a)
    {
        updateEvent -= a;
    }

    void Update()
    {
        updateEvent?.Invoke();
    }
/// <summary>
/// 开启协程 
/// </summary>
/// <param name="routine"></param>
/// <returns></returns>
    public Coroutine startCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }
/// <summary>
/// 关闭协程
/// </summary>
/// <param name="routine"></param>
    public void stopCoroutine(Coroutine routine)
    {
        StopCoroutine(routine);
    }
}