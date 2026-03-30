using System.Collections.Generic;
using UnityEngine;

public class GameManager : UnitySingleTonMono<GameManager>
{
    public EnemyConfigSO enemyConfig;
    // 原始配置（仅读取，不修改）
    private Dictionary<string, EnemyConfig> originEnemyConfigDict = new();
    // 运行时副本（难度调整只改这个）
    public Dictionary<string, EnemyConfigRuntime> RuntimeEnemyConfigDict = new();

    private new void Awake()
    {
        base.Awake();
        InitEnemyConfig();
    }
    
    /// <summary>
    /// 初始化：从SO读取原始配置，创建运行时副本
    /// </summary>
    private void InitEnemyConfig()
    {
        if (enemyConfig == null || enemyConfig.EnemyConfigs == null || enemyConfig.EnemyConfigs.Count == 0)
        {
            Debug.LogError("EnemyConfigSO 未配置或为空！");
            return;
        }
        // 加载原始配置（仅读取，不修改）
        foreach (var config in enemyConfig.EnemyConfigs)
        {
            if (string.IsNullOrEmpty(config.name))
            {
                Debug.LogWarning("存在名称为空的敌人配置！");
                continue;
            }
            if (!originEnemyConfigDict.ContainsKey(config.name))
            {
                originEnemyConfigDict.Add(config.name, config);
            }
        }
        // 创建运行时副本
        foreach (var kvp in originEnemyConfigDict)
        {
            if (!RuntimeEnemyConfigDict.ContainsKey(kvp.Key))
            {
                RuntimeEnemyConfigDict.Add(kvp.Key, new EnemyConfigRuntime(kvp.Value));
            }
        }
    }

    /// <summary>
    /// 对外提供：重置指定敌人的运行时配置为原始值
    /// </summary>
    public void ResetEnemyConfig(string enemyName)
    {
        if (RuntimeEnemyConfigDict.TryGetValue(enemyName, out var config)) { config.ResetToOrigin(); }
    }

    /// <summary>
    /// 对外提供：重置所有敌人的运行时配置为原始值
    /// </summary>
    public void ResetAllEnemyConfig()
    {
        foreach (var config in RuntimeEnemyConfigDict.Values) { config.ResetToOrigin(); }
    }
    
}
