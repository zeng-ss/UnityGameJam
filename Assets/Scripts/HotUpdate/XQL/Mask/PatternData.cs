using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 花纹数据（解锁后选择的增益，对应派系）
/// </summary>
[System.Serializable]
public class PatternData
{
    public int patternId;          // 花纹ID
    public string patternName;     // 花纹名称
    public string patternDesc;     // 花纹描述
    public MaskFaction patternFaction; // 花纹派系
    public List<BuffData> patternBuffs; // 花纹增益
    public bool isRandomEffect;    // 是否为随机效果
}