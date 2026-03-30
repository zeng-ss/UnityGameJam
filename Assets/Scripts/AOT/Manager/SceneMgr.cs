using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// 场景管理器  
/// </summary>
public class SceneMgr:UnitySingleTonMono<SceneMgr>
{
    /// <summary>
    /// 同步切换场景 
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadScene(string sceneName,UnityAction fun=null)
    {
        // 使用 Addressables 加载场景
        var loadHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        loadHandle.Completed += handle => {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                fun?.Invoke();
            }
            else
            {
                Debug.LogError($"Addressables 加载场景失败: {sceneName}，错误：{handle.OperationException}");
            }
        };
    }

    /// <summary>
    /// 异步加载场景的方法  
    /// </summary>  
    /// <param name="sceneName"></param>
    /// <param name="fun"></param>
    public void LoadSceneAsync(string sceneName, UnityAction fun = null)
    {
        StartCoroutine(LoadSceneEnumerator(sceneName,fun));
    }

    private IEnumerator LoadSceneEnumerator(string sceneName,UnityAction fun=null)
    {
        // 先打开加载面板
        //UIManager.Instance.openPanel<LoadPanel>();
        // 等待一帧，确保加载面板已经显示
        yield return null;
        // 使用 Addressables 加载场景
        var loadHandle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        float progress = 0;
        // 使用真实的加载进度
        while (!loadHandle.IsDone)
        {
            progress = loadHandle.PercentComplete;
            // 更新进度条
            EventCenter.Instance.EventTrigger(GameEvent.进度条加载, progress);
            yield return null;
        }
        if (loadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Addressables 加载场景失败: {sceneName}，错误：{loadHandle.OperationException}");
            yield break;
        } 
        // 确保进度条显示100%
        EventCenter.Instance.EventTrigger(GameEvent.进度条加载, 1.0f);
        // 等待一小段时间让玩家看到100%的进度
        // yield return new WaitForSeconds(1f);
        // 执行回调
        fun?.Invoke();
    }
    
    
}
