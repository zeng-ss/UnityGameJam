using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 面具系统核心管理器（单例）
/// 负责：初始面具选择、花纹生成与选择、增益计算、数据存储
/// </summary>
public class MaskSystemManager : UnitySingleTonMono<MaskSystemManager> 
{
    [Header("【面具基础配置】")]
    public List<MaskData> allMaskDatas; // 所有初始面具数据
    public int maxPatternCount = 30;     // 最大花纹数量

    [Header("【花纹基础配置】")]
    public List<PatternData> allPatternDatas; // 所有花纹数据
    public float sameFactionProbability = 0.75f; // 对应面具派系花纹出现概率

    [Header("【变化系配置】")]
    public List<BuffType> randomBuffTypes; // 变化系可随机的增益类型
    public float randomBuffMinValue = 5f;  // 变化系随机增益最小值
    public float randomBuffMaxValue = 15f; // 变化系随机增益最大值

    // 私有数据（存储玩家当前面具状态）
    private MaskFaction _currentMaskFaction; // 当前选中的面具派系
    private List<BuffData> _totalBuffs = new();      // 总增益（面具基础+花纹叠加）
    private List<PatternData> _selectedPatterns = new();  // 已选花纹（最多6个）
    private bool _isMaskSelected;            // 是否已选择初始面具

    private PlayerStats playerStats;
    private PlayerProPanel playerProPanel;

    private void Start()
    {
        playerStats = SceneObjectManager.Instance.GetObjectByTag<PlayerStats>("Player");
    }

    private void Update()
    {
        if (playerProPanel == null)
        {
            playerProPanel = UIManager.Instance.getPanel<PlayerProPanel>();
        }
    }

    #region 公开接口：初始面具选择（UI调用）
    /// <summary>
    /// 选择初始面具（UI按钮点击调用）
    /// </summary>
    /// <param name="faction">选择的面具派系</param>
    public void SelectInitialMask(MaskFaction faction)
    {
        if (_isMaskSelected)
        {
            Debug.LogWarning("已选择过初始面具，无法重复选择！");
            return;
        }

        // 查找对应面具数据
        MaskData targetMask = allMaskDatas.FirstOrDefault(m => m.maskFaction == faction);
        if (targetMask == null)
        {
            Debug.LogError($"未找到{faction}类型的初始面具数据！");
            return;
        }

        // 记录当前面具派系
        _currentMaskFaction = faction;
        _isMaskSelected = true;

        // 处理基础增益
        if (faction == MaskFaction.Random)
        {
            // 变化面具：随机获得1种基础增益
            AddRandomBaseBuff();
        }
        else
        {
            // 疾风/恶鬼面具：添加固定基础增益
            AddBuffsToTotal(targetMask.baseBuffs);
        }

        Debug.Log($"成功选择初始面具：{targetMask.maskName}，获得基础增益！");
        OnMaskOrPatternChanged();
    }
    #endregion

    #region 公开接口：消灭大波敌人后，生成3个可选花纹（敌人管理器调用）
    /// <summary>
    /// 解锁花纹选择（消灭大波敌人后调用）
    /// </summary>
    /// <returns>3个可选花纹列表（用于UI展示）</returns>
    public List<PatternData> UnlockPatternSelection()
    {
        if (!_isMaskSelected)
        {
            Debug.LogWarning("未选择初始面具，无法解锁花纹！");
            return null;
        }

        if (_selectedPatterns.Count >= maxPatternCount)
        {
            Debug.LogWarning("已达到最大花纹数量（6个），无法继续解锁！");
            return null;
        }

        // 生成3个随机花纹（遵循75%同派系概率）
        List<PatternData> optionalPatterns = GenerateOptionalPatterns(3);

        return optionalPatterns;
    }
    #endregion

    #region 公开接口：选择花纹（UI调用）
    /// <summary>
    /// 选择花纹（UI按钮点击调用）
    /// </summary>
    /// <param name="selectedPattern">选择的花纹数据</param>
    public void SelectPattern(PatternData selectedPattern)
    {
        if (!_isMaskSelected)
        {
            Debug.LogWarning("未选择初始面具，无法选择花纹！");
            return;
        }

        if (_selectedPatterns.Count >= maxPatternCount)
        {
            Debug.LogWarning("已达到最大花纹数量（6个），无法继续选择！");
            return;
        }

        if (selectedPattern == null)
        {
            Debug.LogError("选择的花纹数据为空！");
            return;
        }

        // 记录已选花纹
        _selectedPatterns.Add(selectedPattern);

        // 处理花纹增益（变化系花纹需随机生成效果）
        if (selectedPattern.isRandomEffect)
        {
            AddRandomPatternBuff();
        }
        else
        {
            AddBuffsToTotal(selectedPattern.patternBuffs);
        }
        
        // 触发UI更新
        OnMaskOrPatternChanged();
    }
    #endregion

    #region 公开接口：获取当前总增益（玩家控制器调用，用于属性计算）
    /// <summary>
    /// 获取指定属性的总增益值
    /// </summary>
    /// <param name="buffType">需要查询的增益类型</param>
    /// <returns>总增益值（百分比，已叠加所有面具+花纹效果）</returns>
    public float GetTotalBuffValue(BuffType buffType)
    {
        float totalValue = 0f;
        // 累加所有对应类型的增益
        foreach (var buff in _totalBuffs)
        {
            if (buff.buffType == buffType)
            {
                totalValue += buff.buffValue;
            }
        }
        return totalValue;
    }
    #endregion

    #region 私有逻辑：生成可选花纹（遵循75%同派系概率）
    private List<PatternData> GenerateOptionalPatterns(int count)
    {
    List<PatternData> optionalPatterns = new List<PatternData>();
    List<PatternData> sameFactionPatterns = new List<PatternData>();
    List<PatternData> otherFactionPatterns = new List<PatternData>();

    // 分离同派系和其他派系花纹
    foreach (var pattern in allPatternDatas)
    {
        if (pattern == null) continue; // 跳过配置中的null花纹
        if (pattern.patternFaction == _currentMaskFaction)
        {
            sameFactionPatterns.Add(pattern);
        }
        else
        {
            otherFactionPatterns.Add(pattern);
        }
    }

    // 校验花纹数据是否充足（关键：如果同派系花纹为空，就用其他派系补充）
    if (sameFactionPatterns.Count == 0 && otherFactionPatterns.Count == 0)
    {
        Debug.LogError("【面具系统】无可用的有效花纹数据！请在Inspector配置花纹！");
        return optionalPatterns;
    }

    // 确保至少有一个派系有花纹（兜底：如果同派系为空，强制使用其他派系）
    bool forceUseOther = sameFactionPatterns.Count == 0;

    // 生成count个可选花纹（确保每个都是有效数据）
    for (int i = 0; i < count; i++)
    {
        PatternData selectedPattern = null;
        // 随机判断是否选择同派系花纹（75%概率，若同派系无数据则强制选其他）
        bool isSameFaction = !forceUseOther && Random.value <= sameFactionProbability;

        // 确保选中的花纹不为null
        int retryCount = 0;
        while (selectedPattern == null && retryCount < 10) // 最多重试10次，避免死循环
        {
            if (isSameFaction && sameFactionPatterns.Count > 0)
            {
                // 选择同派系花纹（随机抽取）
                selectedPattern = sameFactionPatterns[Random.Range(0, sameFactionPatterns.Count)];
            }
            else if (otherFactionPatterns.Count > 0)
            {
                // 选择其他派系花纹（随机抽取）
                selectedPattern = otherFactionPatterns[Random.Range(0, otherFactionPatterns.Count)];
            }
            else
            {
                // 最后兜底：无论哪个派系，只要有数据就选
                selectedPattern = sameFactionPatterns.Count > 0 ? sameFactionPatterns[0] : otherFactionPatterns[0];
            }
            retryCount++;
        }

        // 避免重复添加相同花纹（可选，若允许重复可注释）
        if (!optionalPatterns.Contains(selectedPattern) && selectedPattern != null)
        {
            optionalPatterns.Add(selectedPattern);
        }
        else
        {
            // 重复则直接添加（优先保证数量为3，避免索引异常）
            if (selectedPattern != null)
            {
                optionalPatterns.Add(selectedPattern);
            }
        }
    }

    // 最终兜底：确保返回的列表数量等于count（3个），不足则补充第一个有效花纹
    while (optionalPatterns.Count < count && optionalPatterns.Count > 0)
    {
        optionalPatterns.Add(optionalPatterns[0]);
    }

    return optionalPatterns;
    }
    #endregion

    #region 私有逻辑：处理增益叠加（基础+花纹）
    // 添加固定增益到总增益
    private void AddBuffsToTotal(List<BuffData> buffs)
    {
        if (buffs == null || buffs.Count == 0) return;

        foreach (var buff in buffs)
        {
            _totalBuffs.Add(new BuffData(buff.buffType, buff.buffValue, buff.isPercent));
        }
    }

    // 变化面具：添加随机基础增益
    private void AddRandomBaseBuff()
    {
        if (randomBuffTypes.Count == 0)
        {
            Debug.LogError("未配置变化系可随机的增益类型！");
            return;
        }

        // 随机选择增益类型和数值
        BuffType randomType = randomBuffTypes[Random.Range(0, randomBuffTypes.Count)];
        float randomValue = Random.Range(randomBuffMinValue, randomBuffMaxValue);
        _totalBuffs.Add(new BuffData(randomType, randomValue, true));

        Debug.Log($"变化面具基础增益：{randomType} +{randomValue:F1}%");
    }

    // 变化系花纹：添加随机增益
    private void AddRandomPatternBuff()
    {
        if (randomBuffTypes.Count == 0)
        {
            Debug.LogError("未配置变化系可随机的增益类型！");
            return;
        }

        // 随机选择增益类型和数值（可设置多个，此处默认1个，可扩展）
        int buffCount = Random.Range(1, 2); // 随机1个增益（可改为2个增强效果）
        for (int i = 0; i < buffCount; i++)
        {
            BuffType randomType = randomBuffTypes[Random.Range(0, randomBuffTypes.Count)];
            float randomValue = Random.Range(randomBuffMinValue, randomBuffMaxValue);
            _totalBuffs.Add(new BuffData(randomType, randomValue, true));

            Debug.Log($"变化系花纹增益：{randomType} +{randomValue:F1}%");
        }
    }
    #endregion

    #region 辅助逻辑：校验配置数据+UI更新回调
    private void CheckConfigData()
    {
        if (allMaskDatas.Count != 3)
        {
            Debug.LogWarning($"初始面具数据数量不为3，当前为{allMaskDatas.Count}！");
        }

        if (allPatternDatas.Count == 0)
        {
            Debug.LogError("未配置任何花纹数据！");
        }

        if (randomBuffTypes.Count == 0)
        {
            Debug.LogError("未配置变化系可随机的增益类型！");
        }
    }
    
    /// <summary>
    /// 面具/花纹变化后，触发UI更新（后续对接UGUI）
    /// </summary>
    private void OnMaskOrPatternChanged()
    {
        if (playerProPanel == null)
        {
            print("xxa");
            return;
        }
        playerStats.CurrentHealth = Mathf.Min(playerProPanel.currentMaxHealth,playerStats.CurrentHealth += 50);
        print("当前血量："+playerStats.GetCurrentHealth());
        EventCenter.Instance.EventTrigger(GameEvent.玩家面板属性变化,playerStats);
    }
    #endregion

    #region 公开属性：获取当前面具状态
    public MaskFaction CurrentMaskFaction => _currentMaskFaction;
    public bool IsMaskSelected
    {
        get { return _isMaskSelected; }
        set { _isMaskSelected = value; }
    }
    public int CurrentPatternCount => _selectedPatterns.Count;
    public int MaxPatternCount => maxPatternCount;
    #endregion
}