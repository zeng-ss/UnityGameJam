using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProPanel : BasePanel
{
    public TMP_Text damaageText;
    public TMP_Text fangyuText;
    public TMP_Text moveSpeedText;
    public TMP_Text shootRateText;
    public Image zhunxinImage;
    private PlayerStats playerStats;
    public float currentMaxHealth;

    private new void Awake()
    {
        playerStats = SceneObjectManager.Instance.GetObjectByTag<PlayerStats>("Player");
    }

    private void OnEnable()
    {
        // 确保playerStats不为null
        if (playerStats == null)
        {
            playerStats = SceneObjectManager.Instance.GetObjectByTag<PlayerStats>("Player");
        }
        
        if (playerStats != null)
        {
            currentMaxHealth = playerStats.GetFinalMaxHealthValue();
            playerStats.CurrentHealth = currentMaxHealth;
            
            // 确保UI组件不为null
            if (damaageText != null)
                damaageText.text = playerStats.attackDamage.ToString(CultureInfo.InvariantCulture);
            if (fangyuText != null)
                fangyuText.text = playerStats.GetFinalMaxHealthValue().ToString(CultureInfo.InvariantCulture);
            if (moveSpeedText != null)
                moveSpeedText.text = playerStats.moveSpeed.ToString(CultureInfo.InvariantCulture);
            if (shootRateText != null)
                shootRateText.text = playerStats.normalShootRate.ToString(CultureInfo.InvariantCulture);
        }
        
        EventCenter.Instance.AddEventListener<PlayerStats>(GameEvent.玩家面板属性变化,UpdateData);
    }

    private void Update()
    {
        zhunxinImage.transform.position = Input.mousePosition;
        
    }

    private void UpdateData(PlayerStats playerStats)
    {
        if (playerStats == null)
            return;
        
        // 确保UI组件不为null
        if (fangyuText != null)
            fangyuText.text = playerStats.GetFinalMaxHealthValue().ToString(CultureInfo.InvariantCulture);
        if (damaageText != null)
            damaageText.text = playerStats.GetFinalAttackDamage().ToString(CultureInfo.InvariantCulture);
        if (moveSpeedText != null)
            moveSpeedText.text = playerStats.GetFinalMoveSpeed().ToString(CultureInfo.InvariantCulture);
        if (shootRateText != null)
            shootRateText.text = playerStats.GetFinalShootRate().ToString(CultureInfo.InvariantCulture);
        
        float old = currentMaxHealth;
        currentMaxHealth = playerStats.GetFinalMaxHealthValue();
        if (!Mathf.Approximately(currentMaxHealth, old))
        {
            print("当前最大血量："+playerStats.GetFinalMaxHealthValue());
        }
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<PlayerStats>(GameEvent.玩家面板属性变化,UpdateData);
    }
    
}