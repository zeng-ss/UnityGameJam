using UnityEngine;

// 动画事件接收转发脚本：挂载到PlayerModle上，接收动画事件并转发给PlayerController + 播放音效
public class PlayerModelAnimEventReceiver : MonoBehaviour
{
    [Tooltip("拖拽Player根节点（挂载PlayerController的对象）")]
    public PlayerController playerController;

    [Header("【音效配置】（新增：音效相关）")]
    [Tooltip("射击音效Clip（普通射击/散射共用）")]
    public AudioClip shootAudioClip;
    [Tooltip("奔跑脚步声Clip（循环播放）")]
    public AudioClip runStepAudioClip;
    [Tooltip("音效播放音量（0-1）")]
    [Range(0f, 1f)] public float audioVolume = 0.5f;

    // 私有组件：音频源（用于播放音效，自动添加）
    private AudioSource _audioSource;
    // 私有标记：是否正在播放脚步声（避免重复播放/停止）
    private bool _isPlayingStepSound;

    private void Awake()
    {
        // 校验引用（避免忘记拖拽）
        if (playerController == null)
        {
            Debug.LogWarning("PlayerModle未关联PlayerController！请将Player根节点拖拽到playerController字段中。");
            // 自动查找（可选，简化配置）
            playerController = FindObjectOfType<PlayerController>();
        }

        // 新增：自动添加AudioSource组件，配置默认参数
        InitAudioSource();
    }

    #region 新增：初始化音频源组件
    private void InitAudioSource()
    {
        // 获取或添加AudioSource组件
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 配置音频源默认参数
        _audioSource.volume = audioVolume;
        _audioSource.loop = false; // 默认不循环，脚步声单独设置循环
        _audioSource.playOnAwake = false; // 不自动播放
    }
    #endregion

    #region 普通射击动画事件（转发给PlayerController + 播放射击音效）
    /// <summary>
    /// 普通射击开火帧事件（动画事件直接调用此方法）
    /// </summary>
    public void OnNormalShootAnimEvent()
    {
        if (playerController != null)
        {
            playerController.OnNormalShootAnimEvent();
        }

        // 新增：播放普通射击音效（与开火动画同步）
        PlayShootAudio();
    }

    /// <summary>
    /// 普通射击动画结束事件（动画事件直接调用此方法）
    /// </summary>
    public void OnNormalShootEndAnimEvent()
    {
        if (playerController != null)
        {
            playerController.OnNormalShootEndAnimEvent();
        }
    }
    #endregion

    #region 散射射击动画事件（转发给PlayerController + 播放射击音效）
    /// <summary>
    /// 散射射击开火帧事件（动画事件直接调用此方法）
    /// </summary>
    public void OnBurstShootAnimEvent()
    {
        if (playerController != null)
        {
            playerController.OnBurstShootAnimEvent();
        }

        // 新增：播放散射射击音效（与开火动画同步，可单独配置音效Clip）
        PlayShootAudio();
    }

    /// <summary>
    /// 散射射击动画结束事件（动画事件直接调用此方法）
    /// </summary>
    public void OnBurstShootEndAnimEvent()
    {
        if (playerController != null)
        {
            playerController.OnBurstShootEndAnimEvent();
        }
    }
    #endregion

    #region 新增：奔跑脚步声动画事件（接收移动动画事件，播放/停止脚步声）
    /// <summary>
    /// 奔跑开始事件（移动动画播放时调用，动画事件绑定）
    /// </summary>
    public void OnRunStepStart()
    {
        if (runStepAudioClip == null || _isPlayingStepSound) return;

        // 设置脚步声循环播放
        _audioSource.loop = true;
        _audioSource.clip = runStepAudioClip;
        _audioSource.Play();

        // 更新标记
        _isPlayingStepSound = true;
        // Debug.Log("开始播放脚步声");
    }

    /// <summary>
    /// 奔跑结束事件（移动动画停止时调用，动画事件绑定）
    /// </summary>
    public void OnRunStepEnd()
    {
        if (!_isPlayingStepSound) return;

        // 停止脚步声播放
        _audioSource.Stop();
        _audioSource.loop = false; // 恢复默认不循环

        // 更新标记
        _isPlayingStepSound = false;
        // Debug.Log("停止播放脚步声");
    }
    #endregion

    #region 新增：音效播放工具方法
    /// <summary>
    /// 播放射击音效（避免音效重叠，保证每次开火只播放一次）
    /// </summary>
    private void PlayShootAudio()
    {
        if (shootAudioClip == null) return;

        // 方式1：直接播放（支持重叠，适合快速连射）
        _audioSource.PlayOneShot(shootAudioClip, audioVolume);

        // 方式2：不重叠播放（适合慢速射击，注释掉方式1可启用）
        // if (!_audioSource.isPlaying)
        // {
        //     _audioSource.clip = shootAudioClip;
        //     _audioSource.Play();
        // }
    }
    #endregion
}