using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 面具选择UI管理器
/// 挂载在MaskSelectCanvas上，负责显示面板、按钮点击、隐藏面板
/// </summary>
public class MaskSelectUIManager : MonoBehaviour
{
    [Header("【按钮配置】")]
    [Tooltip("按钮0：疾风面具（Btn_Wind）")]
    public Button btn_Wind;
    [Tooltip("按钮1：恶鬼面具（Btn_Oni）")]
    public Button btn_Oni;
    [Tooltip("按钮2：变化面具（Btn_Random）")]
    public Button btn_Random;

    [Header("【面板配置】")]
    [Tooltip("面具选择面板（MaskSelectPanel）")]
    public GameObject maskSelectPanel;

    // 单例实例（便于其他脚本调用显示面板方法）
    public static MaskSelectUIManager Instance;

    private void Awake()
    {
        // 单例模式：确保全局唯一，便于其他脚本调用
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 绑定按钮点击事件
        BindButtonEvents();

        // 初始化：默认隐藏面板（可选，若UI已手动隐藏，此步骤可省略）
        if (maskSelectPanel != null)
        {
            maskSelectPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 绑定3个按钮的点击事件
    /// </summary>
    private void BindButtonEvents()
    {
        if (btn_Wind != null)
        {
            // 按钮0：选择疾风面具
            btn_Wind.onClick.AddListener(() =>
            {
                SelectMask(MaskFaction.Wind);
            });
        }

        if (btn_Oni != null)
        {
            // 按钮1：选择恶鬼面具
            btn_Oni.onClick.AddListener(() =>
            {
                SelectMask(MaskFaction.Oni);
            });
        }

        if (btn_Random != null)
        {
            // 按钮2：选择变化面具
            btn_Random.onClick.AddListener(() =>
            {
                SelectMask(MaskFaction.Random);
            });
        }
    }

    /// <summary>
    /// 选择面具的核心逻辑
    /// </summary>
    /// <param name="faction">选择的面具派系</param>
    private void SelectMask(MaskFaction faction)
    {
        //调用面具系统管理器，确认选择该面具
        if (MaskSystemManager.Instance != null)
        {
            MaskSystemManager.Instance.SelectInitialMask(faction);
        }
        //隐藏面具选择面
        HideMaskSelectPanel();
    }

    /// <summary>
    /// 公开方法：显示面具选择面板（游戏运行时调用，如游戏启动时）
    /// </summary>
    public void ShowMaskSelectPanel()
    {
        if (maskSelectPanel != null)
        {
            // 确保面板激活显示
            maskSelectPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("未配置面具选择面板（maskSelectPanel）！");
        }
    }

    /// <summary>
    /// 私有方法：隐藏面具选择面板
    /// </summary>
    private void HideMaskSelectPanel()
    {
        if (maskSelectPanel != null)
        {
            maskSelectPanel.SetActive(false);
        }
    }
}