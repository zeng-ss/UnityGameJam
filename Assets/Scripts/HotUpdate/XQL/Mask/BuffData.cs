using UnityEngine;

/// <summary>
/// 增益数据（单个属性的加成值）
/// </summary>
[System.Serializable]
public class BuffData
{
    public BuffType buffType; // 增益类型
    public float buffValue;   // 增益数值（百分比/固定值，这里统一用百分比，便于计算）
    public bool isPercent;    // 是否为百分比加成（true：乘算，false：加算）

    // 构造函数
    public BuffData(BuffType type, float value, bool IsPercent = true)
    {
        buffType = type;
        buffValue = value;
        isPercent = IsPercent;
    }
}