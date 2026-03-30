using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Config/EnemyConfig")]
public class EnemyConfigSO : ScriptableObject
{
    public List<EnemyConfig> EnemyConfigs;
}

[System.Serializable]
public class EnemyConfig
{
    public string name;
    public float damage;
    public float moveSpeed;
    public float health;
}

public class EnemyConfigRuntime
{
    public string name;
    public float damage;       // 运行时伤害（可修改）
    public float moveSpeed;    // 运行时移速（可修改）
    public float health;       // 运行时血量（可修改）

    // 原始配置备份（用于重置/对比）
    private float _originDamage;
    private float _originMoveSpeed;
    private float _originHealth;

    // 从SO配置创建运行时副本，并备份原始值
    public EnemyConfigRuntime(EnemyConfig soConfig)
    {
        name = soConfig.name;
        // 初始化运行时属性（和原始配置一致）
        damage = soConfig.damage;
        moveSpeed = soConfig.moveSpeed;
        health = soConfig.health;
        // 备份原始值（可选，用于重置）
        _originDamage = soConfig.damage;
        _originMoveSpeed = soConfig.moveSpeed;
        _originHealth = soConfig.health;
    }

    // 重置为原始配置（比如新波次不需要继承上一波的属性）
    public void ResetToOrigin()
    {
        damage = _originDamage;
        moveSpeed = _originMoveSpeed;
        health = _originHealth;
    }
}

// 游戏操作枚举（根据游戏需求扩展）
public enum GameAction
{
    None,
    Drop,      // 丢弃道具
    Jump,      // 跳跃
    Attack,    // 攻击
    Crouch,    // 下蹲
    PickUp,    // 捡起
    Interact,  // 交互
    UsingItem, // 使用道具映射
}

// 按键绑定数据
public class KeyBinding
{
    public GameAction action;       // 对应操作
    public KeyCode bindedKey;       // 仅绑定单个按键
    public string actionName;

    // 构造函数（默认绑定单个按键）
    public KeyBinding(GameAction action, KeyCode defaultKeys,string actionName)
    {
        this.action = action;
        bindedKey = defaultKeys;
        this.actionName = actionName;
    }
}


// 存档的数据
public class MainSaveData
{
    public string name; // 存档名字
    public int coin;    // 玩家金币数量数据
    public long saveRealTicks; // 存档时的真实系统时间戳
    public float GlobalSoundVolumeData; // 全局音量
    public bool isFirstPlayGameData;    // 判断是否为第一次玩游戏数据
    public Dictionary<GameAction,KeyBinding> KeyBindingsDictData;
    public Dictionary<int, int> ItemHasNumDictData;
    public Dictionary<int, bool> ItemLockDictData;  // 所有道具的解锁情况
    public Dictionary<int, bool> GhostLockDictData; // 判断是否解锁鬼魂字典
}




