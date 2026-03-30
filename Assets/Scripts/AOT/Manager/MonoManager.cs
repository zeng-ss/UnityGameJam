using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共的momo管理器  其他没有继承Momobehavior的类也可以使用协程 还有调用update方法
/// </summary>
public class MonoManager : UnitySingleTonMono<MonoManager>
{
    private Action updateAction;
    private Action lateUpdateAction;
    private Action FixedUpdateAction;

    public void AddUpdateListener(Action action)
    {
        updateAction += action;
    }

    public void RemoveUpdateListener(Action action)
    {
        updateAction -= action;
    }

    public void AddFixedUpdateListener(Action action)
    {
        FixedUpdateAction += action;
    }

    public void RemoveFixedUpdateListener(Action action)
    {
        FixedUpdateAction -= action;
    }

    public void AddLateUpdateListener(Action action)
    {
        lateUpdateAction += action;
    }

    public void RemoveLateUpdateListener(Action action)
    {
        lateUpdateAction -= action;
    }


    private void Update()
    {
        updateAction?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpdateAction?.Invoke();
    }

    private void FixedUpdate()
    {
        FixedUpdateAction?.Invoke();
    }



}
