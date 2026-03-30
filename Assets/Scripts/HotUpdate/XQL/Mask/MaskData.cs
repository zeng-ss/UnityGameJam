using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 面具基础数据（初始面具的基础增益）
/// </summary>
[System.Serializable]
public class MaskData
{
    public MaskFaction maskFaction; // 面具派系
    public string maskName;         // 面具名称
    public string maskDesc;         // 面具描述
    public List<BuffData> baseBuffs; // 基础增益
}