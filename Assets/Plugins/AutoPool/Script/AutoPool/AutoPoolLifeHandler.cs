using System;
using System.Collections;
using UnityEngine;

namespace AutoPool_Tool
{
    public class AutoPoolLifeHandler
    {
        MainAutoPool _autoPool;
        float _maxTimer = 600f;
        int _delayTime = 10; // seconds
        public AutoPoolLifeHandler(MainAutoPool autoPool)
        {
            _autoPool = autoPool;
        }

        public IEnumerator IsActiveRoutine(int id)
        {
            float delayTime = _delayTime;
            float timer = _maxTimer;
            while (true)
            {
                if (_autoPool.PoolDic[id].IsUsed == true)
                {
                    timer = _maxTimer;
                    PoolInfo pool = _autoPool.PoolDic[id];
                    pool.IsUsed = false;
                    pool.IsActive = true;
                }
                if (timer <= 0)
                {
                    _autoPool.ClearPool(_autoPool.PoolDic[id]);
                }
                else
                {
                    timer -= delayTime;
                }
                yield return _autoPool.Second(delayTime);
            }
        }

        public IEnumerator IsActiveGenericRoutine<T>() where T : class, IPoolGeneric, new()
        {
            Type type = typeof(T);
            float delayTime = _delayTime;
            float timer = _maxTimer;
            while (true)
            {
                if (_autoPool.GenericPoolDic[type].IsUsed == true)
                {
                    timer = _maxTimer;
                    GenericPoolInfo pool = _autoPool.GenericPoolDic[type];
                    pool.IsUsed = false;
                    pool.IsActive = true;
                }
                if (timer <= 0)
                {
                    _autoPool.ClearGenericPool<T>();
                }
                else
                {
                    timer -= delayTime;
                }
                yield return _autoPool.Second(delayTime);
            }

        }
    }
}