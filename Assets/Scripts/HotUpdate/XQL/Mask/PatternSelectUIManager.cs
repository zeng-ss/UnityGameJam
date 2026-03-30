using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 花纹选择UI管理器（3选1）
/// 挂载在PatternSelectCanvas上，负责面板显隐、花纹展示、选择转发
/// </summary>
public class PatternSelectUIManager : MonoBehaviour
{
    [Header("【花纹选择按钮配置】")]
    [Tooltip("花纹选择按钮1（3选1第一个）")]
    public Button btn_Pattern1;
    [Tooltip("花纹选择按钮2（3选1第二个）")]
    public Button btn_Pattern2;
    [Tooltip("花纹选择按钮3（3选1第三个）")]
    public Button btn_Pattern3;

    [Header("【花纹信息显示配置（可选，UI展示用）】")]
    [Tooltip("按钮1的花纹名称文本")]
    public Text txt_Pattern1Name;
    [Tooltip("按钮1的花纹描述文本")]
    public Text txt_Pattern1Desc;
    [Tooltip("按钮2的花纹名称文本")]
    public Text txt_Pattern2Name;
    [Tooltip("按钮2的花纹描述文本")]
    public Text txt_Pattern2Desc;
    [Tooltip("按钮3的花纹名称文本")]
    public Text txt_Pattern3Name;
    [Tooltip("按钮3的花纹描述文本")]
    public Text txt_Pattern3Desc;

    [Header("【面板配置】")]
    [Tooltip("花纹选择主面板")]
    public GameObject patternSelectPanel;

    // 单例实例：全局调用显示面板
    public static PatternSelectUIManager Instance;
    // 临时存储当前可选的3个花纹数据
    private List<PatternData> _currentOptionalPatterns;

    private void Awake()
    {
        // 单例初始化（全局唯一）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化可选花纹列表
        _currentOptionalPatterns = new List<PatternData>();
        // 绑定按钮点击事件
        BindButtonEvents();
        // 默认隐藏面板
        if (patternSelectPanel != null)
        {
            patternSelectPanel.SetActive(false);
        }

        // 校验按钮配置
        CheckButtonConfig();
    }

    /// <summary>
    /// 校验按钮是否赋值，避免空引用
    /// </summary>
    private void CheckButtonConfig()
    {
        if (btn_Pattern1 == null || btn_Pattern2 == null || btn_Pattern3 == null)
        {
            Debug.LogError("花纹选择按钮未完全赋值！请在Inspector面板拖拽按钮对象！");
        }
    }

    /// <summary>
    /// 绑定3个花纹按钮的点击事件
    /// </summary>
    private void BindButtonEvents()
    {
        btn_Pattern1?.onClick.AddListener(() => SelectPattern(0));
        btn_Pattern2?.onClick.AddListener(() => SelectPattern(1));
        btn_Pattern3?.onClick.AddListener(() => SelectPattern(2));
    }

    /// <summary>
    /// 选择花纹核心逻辑（根据索引选择）
    /// </summary>
    /// <param name="index">0=按钮1，1=按钮2，2=按钮3</param>
    private void SelectPattern(int index)
    {
        // 校验索引和花纹数据合法性
        if (index < 0 || index >= _currentOptionalPatterns.Count)
        {
            Debug.LogError("花纹选择索引超出范围！");
            return;
        }
        PatternData selectedPattern = _currentOptionalPatterns[index];
        if (selectedPattern == null)
        {
            Debug.LogError("选中的花纹数据为空！");
            return;
        }

        // 调用面具系统管理器，完成花纹选择+增益叠加
        MaskSystemManager.Instance?.SelectPattern(selectedPattern);
        // 隐藏花纹选择面板
        HidePatternSelectPanel();
        // 清空当前可选花纹数据
        _currentOptionalPatterns.Clear();
    }

    /// <summary>
    /// 公开方法：显示花纹选择面板（敌人管理器消灭大波敌人后调用）
    /// </summary>
    /// <param name="optionalPatterns">面具系统生成的3个可选花纹</param>
    public void ShowPatternSelectPanel(List<PatternData> optionalPatterns)
    {
        // 1. 先清空旧数据，避免残留
        _currentOptionalPatterns.Clear();

        // 2. 分步校验，打印详细日志（方便排查问题）
        if (patternSelectPanel == null)
        {
            Debug.LogError("【花纹UI】花纹选择面板未赋值！请在Inspector拖拽PatternSelectPanel！");
            return;
        }

        if (optionalPatterns == null)
        {
            Debug.LogError("【花纹UI】传入的可选花纹列表为null！请检查面具系统是否生成了有效花纹！");
            return;
        }

        if (optionalPatterns.Count != 3)
        {
            Debug.LogError($"【花纹UI】传入的花纹数量不对，当前是{optionalPatterns.Count}个，需要3个！");
            return;
        }

        // 3. 校验每个花纹数据是否有效（避免列表有3个null元素）
        foreach (var pattern in optionalPatterns)
        {
            if (pattern == null)
            {
                Debug.LogError("【花纹UI】传入的花纹列表中包含null元素！请检查面具系统的花纹配置！");
                _currentOptionalPatterns.Clear();
                return;
            }
        }

        // 4. 确认数据有效后，再存储并显示面板
        _currentOptionalPatterns = new List<PatternData>(optionalPatterns); // 深拷贝，避免外部数据修改影响
        patternSelectPanel.SetActive(true);
        UpdatePatternUIInfo();
    }

    /// <summary>
    /// 更新花纹名称/描述到UI文本（强化校验，确保不报错）
    /// </summary>
    private void UpdatePatternUIInfo()
    {
        // 先校验数据
        if (_currentOptionalPatterns == null || _currentOptionalPatterns.Count != 3)
        {
            Debug.LogWarning("【花纹UI】可选花纹数量不是3个，无法更新UI文本！");
            return;
        }

        // 给3个按钮的文本赋值（添加空值校验，避免报错）
        SetPatternText(txt_Pattern1Name, txt_Pattern1Desc, _currentOptionalPatterns[0]);
        SetPatternText(txt_Pattern2Name, txt_Pattern2Desc, _currentOptionalPatterns[1]);
        SetPatternText(txt_Pattern3Name, txt_Pattern3Desc, _currentOptionalPatterns[2]);
    }

    /// <summary>
    /// 工具方法：设置单个花纹的名称和描述文本（强化兜底）
    /// </summary>
    private void SetPatternText(Text nameTxt, Text descTxt, PatternData pattern)
    {
        // 校验花纹数据
        if (pattern == null)
        {
            Debug.LogWarning("【花纹UI】花纹数据为空，无法设置文本！");
            if (nameTxt != null) nameTxt.text = "未知花纹";
            if (descTxt != null) descTxt.text = "无描述";
            return;
        }

        // 赋值名称文本
        if (nameTxt != null)
        {
            nameTxt.text = string.IsNullOrEmpty(pattern.patternName) ? "未知花纹" : pattern.patternName;
        }

        // 赋值描述文本
        if (descTxt != null)
        {
            descTxt.text = string.IsNullOrEmpty(pattern.patternDesc) ? "无描述" : pattern.patternDesc;
        }
    }

    /// <summary>
    /// 隐藏花纹选择面板
    /// </summary>
    public void HidePatternSelectPanel()
    {
        if (patternSelectPanel != null)
        {
            patternSelectPanel.SetActive(false);
        }
    }
    /// <summary>
    /// 消灭大波敌人后触
    /// </summary>
    public void OnWaveEnemyDestroyed()
    {
        // 1. 先校验面具系统是否初始化完成，且已选择初始面具
        if (MaskSystemManager.Instance == null)
        {
            Debug.LogError("【敌人管理器】MaskSystemManager实例不存在！");
            return;
        }

        if (!MaskSystemManager.Instance.IsMaskSelected)
        {
            Debug.LogWarning("【敌人管理器】玩家尚未选择初始面具，无法解锁花纹！");
            return;
        }

        if (MaskSystemManager.Instance.CurrentPatternCount >= MaskSystemManager.Instance.MaxPatternCount)
        {
            Debug.LogWarning($"【敌人管理器】已达到最大花纹数量（{MaskSystemManager.Instance.MaxPatternCount}个），无法继续解锁！");
            return;
        }

        // 2. 调用面具系统生成3个可选花纹
        List<PatternData> optionalPatterns = MaskSystemManager.Instance.UnlockPatternSelection();

        // 3. 校验花纹数据并显示UI
        if (Instance == null)
        {
            Debug.LogError("【敌人管理器】PatternSelectUIManager实例不存在！");
            return;
        }

        if (optionalPatterns != null && optionalPatterns.Count == 3)
        {
            ShowPatternSelectPanel(optionalPatterns);
        }
        else
        {
            Debug.LogError($"【敌人管理器】生成的花纹数据无效，数量：{optionalPatterns?.Count ?? 0}");
        }
    }
}