using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    #region 玩家核心数值（可在Inspector面板配置初始值）
    [Header("生命数值")]
    [Tooltip("玩家最大血量")]
    public float maxHealth = 100f;
    [Tooltip("玩家当前血量（运行时动态变化，初始值等于最大血量）")]
    private float _currentHealth;

    public float CurrentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }

    [Header("移动数值")]
    [Tooltip("玩家基础移动速度（供PlayerController调用）")]
    public float moveSpeed = 6f;

    [Header("射击数值（核心优化：攻速=1秒内可发射子弹数）")]
    [Tooltip("普通射击基础攻速（1秒内可发射的子弹数，默认1=1秒1发）")]
    public float normalShootRate = 1f; // 优化：默认改为1，更直观
    [Tooltip("散射射击基础攻速（1秒内可发射的子弹数，默认0.5=2秒1发，比普通慢）")]
    public float burstShootRate = 0.5f; // 优化：默认改为0.5，更直观
    [Tooltip("射速倍率（放大基础攻速，最终攻速=基础攻速×倍率，默认1=无放大）")]
    public float shootRateMultiplier = 1f;
    [Tooltip("射击攻击力")]
    public float attackDamage = 10f;
    [Tooltip("普通射击弹道数（默认1，发射对应子弹数）")]
    public int normalBallisticCount = 1;
    [Tooltip("散射射击弹道数（1=5~10发，2=10~20发，以此类推）")]
    public int burstBallisticCount = 1;
    [Tooltip("子弹最大射程（超出则自动销毁）")]
    public float bulletMaxRange = 20f;
    [Tooltip("普通射击子弹飞行速度（快）")]
    public float normalBulletSpeed = 30f;
    [Tooltip("散射射击子弹飞行速度（慢）")]
    public float burstBulletSpeed = 15f;

    [Header("其他预留数值")]
    [Tooltip("玩家当前护甲值（减免伤害，预留扩展）")]
    public float armor;
    [Tooltip("拖拽场景中的PlayerModel对象（玩家可视化模型）")]
    public PlayerModle playerModel; // 新增：玩家模型引用
    #endregion

    #region 初始化（给当前血量赋值，确保运行时数值合法）
    private void Awake()
    {
        // 初始化当前血量为最大血量，避免初始血量为0
        _currentHealth = maxHealth;

        // 校验弹道数（不能小于1）
        normalBallisticCount = Mathf.Max(normalBallisticCount, 1);
        burstBallisticCount = Mathf.Max(burstBallisticCount, 1);
        // 校验攻速相关数值（不能小于0.01，避免冷却时间为无限大）
        normalShootRate = Mathf.Max(normalShootRate, 0.01f);
        burstShootRate = Mathf.Max(burstShootRate, 0.01f);
        shootRateMultiplier = Mathf.Max(shootRateMultiplier, 0.01f);
    }
    #endregion

    #region 公共方法：生命数值修改（核心，保证数值合法性）
    /// <summary>
    /// 玩家加血（如拾取血包）
    /// </summary>
    /// <param name="healValue">加血数值</param>
    public void HealHealth(float healValue)
    {
        // 避免加血数值为负数，且当前血量不超过最大血量
        if (healValue <= 0) return;
        _currentHealth = Mathf.Min(_currentHealth + healValue, maxHealth);
    }

    /// <summary>
    /// 玩家扣血（如被敌人攻击）
    /// </summary>
    /// <param name="damageValue">扣血数值（已扣除护甲减免）</param>
    public void TakeDamage(float damageValue)
    {
        // 避免扣血数值为负数，且当前血量不低于0
        if (damageValue <= 0) return;
        // 护甲减免伤害（简单逻辑：最终伤害 = 原始伤害 - 护甲值，最低为1）
        float finalDamage = Mathf.Max(damageValue - armor, 1f);
        _currentHealth = Mathf.Max(_currentHealth - finalDamage, 0f);
        EventCenter.Instance.EventTrigger(GameEvent.玩家面板属性变化,this);
        // 预留：当前血量为0时，触发玩家死亡逻辑
        if (_currentHealth <= 0)
        {
            OnPlayerDie();
        }
    }

    private bool isPlayerDead;
    /// <summary>
    /// 玩家死亡逻辑
    /// </summary>
    private void OnPlayerDie()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false; // 禁用玩家控制器，停止移动/射击输入
        }
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in audioSources)
        {
            audio.Stop();
        }
        if(!isPlayerDead)
        {
            isPlayerDead = true;
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.playerDieClip, transform.position);
        }
        playerModel.Animator.SetBool("IsDie", true);
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2);
        UIManager.Instance.openPanel<FailPanel>();
    }

    #endregion

    #region 公共方法：获取私有数值（供其他脚本调用，如当前血量）
    /// <summary>
    /// 获取当前血量（只读，避免外部修改）
    /// </summary>
    /// <returns>当前血量值</returns>
    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    /// <summary>
    /// 获取最大血量（只读）
    /// </summary>
    /// <returns>最大血量值</returns>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 获取当前血量百分比（用于制作血条UI，0~1之间）
    /// </summary>
    /// <returns>血量百分比</returns>
    public float GetHealthPercent()
    {
        return _currentHealth / maxHealth;
    }
    #endregion

    #region 公共方法：数值临时修改（预留，如增益buff、减速debuff）
    /// <summary>
    /// 临时修改移动速度（如加速buff）
    /// </summary>
    /// <param name="newMoveSpeed">新的移动速度</param>
    /// <param name="duration">buff持续时间（秒，0为永久修改）</param>
    public void ModifyMoveSpeed(float newMoveSpeed, float duration = 0f)
    {
        // 避免移动速度为负数
        if (newMoveSpeed <= 0) newMoveSpeed = 1f;

        // 永久修改移动速度
        if (duration <= 0)
        {
            moveSpeed = newMoveSpeed;
            Debug.Log($"玩家移动速度永久修改为：{moveSpeed}");
            return;
        }

        // 临时修改移动速度（后续可通过协程实现buff持续时间，预留扩展）
        Debug.Log($"玩家移动速度临时修改为：{newMoveSpeed} | 持续时间：{duration}秒");
        // 示例协程（需开启using System.Collections;）：
        // StartCoroutine(MoveSpeedBuffCoroutine(newMoveSpeed, duration));
    }
    // 公开方法：获取叠加面具增益后的属性值
    public float GetFinalMoveSpeed()
    {
        if (!MaskSystemManager.Instance.IsMaskSelected) return moveSpeed;
        // 叠加移动速度增益（百分比）
        float buffValue = MaskSystemManager.Instance.GetTotalBuffValue(BuffType.MoveSpeed);
        return Mathf.Round(moveSpeed * (1 + buffValue / 100f) * 10f) / 10f;
    }

    public float GetFinalShootRate()
    {
        if (!MaskSystemManager.Instance.IsMaskSelected) return normalShootRate;
        // 叠加射速增益（百分比）
        float buffValue = MaskSystemManager.Instance.GetTotalBuffValue(BuffType.ShootRate);
        return Mathf.Round(normalShootRate * (1 + buffValue / 100f) * shootRateMultiplier * 10f) / 10f;
    }

    public float GetFinalAttackDamage()
    {
        if (!MaskSystemManager.Instance.IsMaskSelected) return attackDamage;
        // 叠加攻击力增益（百分比）
        float buffValue = MaskSystemManager.Instance.GetTotalBuffValue(BuffType.AttackDamage);
        return Mathf.Round(attackDamage * (1 + buffValue / 100f) * 10f) / 10f;
    }

    public float GetFinalMaxHealth()
    {
        if (!MaskSystemManager.Instance.IsMaskSelected) return maxHealth;
        // 叠加最大生命值增益（百分比）
        float buffValue = MaskSystemManager.Instance.GetTotalBuffValue(BuffType.MaxHealth);
        // 保留一位小数
        maxHealth = Mathf.Round(maxHealth * (1 + buffValue / 100f) * 10f) / 10f;
        return maxHealth;
    }
    
    public float GetFinalMaxHealthValue()
    {
        if (!MaskSystemManager.Instance.IsMaskSelected) return maxHealth;
        // 叠加最大生命值增益（百分比）
        float buffValue = MaskSystemManager.Instance.GetTotalBuffValue(BuffType.MaxHealth);
        return Mathf.Round(maxHealth * (1 + buffValue / 100f) * 10f) / 10f;
    }
    
    #endregion
}