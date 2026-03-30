using UnityEngine;

/// <summary>
/// 面具派系枚举（对应3种初始面具）
/// </summary>
public enum MaskFaction
{
    None,
    Wind, // 疾风面具（射速+移速）
    Oni,  // 恶鬼面具（攻击力+生命值）
    Random // 变化面具（随机效果）
}

/// <summary>
/// 增益类型枚举（对应所有可叠加的属性）
/// </summary>
public enum BuffType
{
    ShootRate,    // 射速
    MoveSpeed,    // 移动速度
    AttackDamage, // 攻击力
    MaxHealth,    // 最大生命值
    BulletSpeed,  // 子弹速度（额外扩展，用于变化系）
    BurstRange,   // 散射范围（额外扩展，用于变化系）
    CoolDownReduce // 冷却缩减（额外扩展，用于变化系）
}