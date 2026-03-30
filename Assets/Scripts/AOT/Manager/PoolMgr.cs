using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 池子数据 
/// </summary>
public class PoolData //小池子 
{
    //池子中的父容器   
    public GameObject fatherObj;
    //池子中放置容器的列表 
    public List<GameObject> poolList;

    public PoolData(GameObject obj, GameObject grandFatherObj)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = grandFatherObj.transform;
        poolList = new List<GameObject>();
        //添加新对象到池子容器列表中 并添加到fatherObj对象下面
        PushObj(obj);
    }

    /// <summary>
    /// 往池子中的放东西  游戏对象用完了
    /// </summary>
    /// <param name="obj"></param>
    public void PushObj(GameObject obj)
    {
        obj.SetActive(false);
        poolList.Add(obj);
        obj.transform.parent = fatherObj.transform;
    }

    /// <summary>
    /// 从池子中拿对象 
    /// </summary>
    /// <returns></returns>
    public GameObject PopObj()
    {
        GameObject obj = poolList[0];
        poolList.RemoveAt(0);
        obj.transform.parent = null;
        obj.SetActive(true);
        return obj;
    }
}

/// <summary>
/// 缓存池管理器
/// </summary>
public class PoolMgr : SingleTon<PoolMgr>
{
    //缓存池容器 
    Dictionary<string, PoolData> poolDic = new();
    private GameObject grandFatherObj;

    /// <summary>
    /// 同步获取对象（仅当池子中有对象时可用，无对象则返回null）
    /// </summary>
    /// <param name="name">Addressables Key</param>
    /// <returns>池子中的对象 / null</returns>
    public GameObject GetObj(string name)
    {
        // 判断字典中是否有该对象对应的池子 并且池子列表长度大于0
        if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
        {
            return poolDic[name].PopObj();
        }
        // 池子中无对象时返回null（异步加载无法同步返回，需调用异步版本）
        Debug.LogWarning($"池子中无{name}对象，请调用GetObjAsync方法异步获取");
        return null;
    }

    /// <summary>
    /// 异步获取对象（推荐：兼容池子有/无对象的情况）
    /// </summary>
    /// <param name="name">Addressables Key</param>
    /// <param name="onComplete">获取完成回调</param>
    public void GetObjAsync(string name, Action<GameObject> onComplete)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("GetObjAsync: 对象名称不能为空！");
            onComplete?.Invoke(null);
            return;
        }

        // 1. 池子中有对象：直接返回
        if (poolDic.ContainsKey(name) && poolDic[name].poolList.Count > 0)
        {
            onComplete?.Invoke(poolDic[name].PopObj());
            return;
        }

        // 2. 池子中无对象：异步加载并实例化
        ResMgr.Instance.LoadAndInstantiateAsync(name, null, obj =>
        {
            if (obj == null)
            {
                Debug.LogError($"加载{name}对象失败，obj为空");
                onComplete?.Invoke(null);
                return;
            }
            // 修正：去掉return obj（Action无返回值），直接给回调传值
            obj.name = name;
            onComplete?.Invoke(obj);
        });
    }

    /// <summary>
    /// 将不用的对象还给池子  
    /// </summary>
    /// <param name="name">对象名称（Addressables Key）</param>
    /// <param name="obj">要回收的对象</param>
    public void PushObj(string name, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("PushObj: 要回收的对象不能为空！");
            return;
        }

        if (grandFatherObj == null)
            grandFatherObj = new GameObject("Pool");

        // 判断是否有池子 
        if (poolDic.ContainsKey(name))
        {
            poolDic[name].PushObj(obj);
        }
        else
        {
            poolDic.Add(name, new PoolData(obj, grandFatherObj));
        }
    }

    /// <summary>
    /// 清空池子容器
    /// </summary>
    public void Clear()
    {
        // 1. 首先销毁所有池子中的物体
        foreach (var pool in poolDic.Values)
        {
            // 销毁父物体（包含所有子物体）
            if (pool.fatherObj != null)
                Destroy(pool.fatherObj);

            // 清空列表引用
            pool.poolList.Clear();
        }

        // 2. 清空字典
        poolDic.Clear();

        // 3. 销毁总父物体
        if (grandFatherObj != null)
            Destroy(grandFatherObj);

        grandFatherObj = null;
    }
}