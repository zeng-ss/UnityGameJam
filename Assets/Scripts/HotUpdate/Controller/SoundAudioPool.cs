using UnityEngine;
using AutoPool_Tool;

public class SoundAudioPool : MonoBehaviour
{
    public AudioClip singleFireClip;
    public AudioClip multiFireClip;
    [Header("发射炮弹")] public AudioClip parabolicClip;
    [Header("炮弹炸开")] public AudioClip parabolicExploClip;
    [Header("机器人死亡爆炸")] public AudioClip robotExploClip;
    [Header("模板弹出")] public AudioClip showPanelClip;
    [Header("按钮选择")] public AudioClip btnSeletClip;
    [Header("面板关闭")] public AudioClip panelHideClip;
    [Header("警报声音")] public AudioClip warningClip;
    [Header("鼠标悬停按钮")] public AudioClip hoverClip;
    [Header("玩家受击")] public AudioClip hitPlayerClip;
    [Header("兽人死亡")] public AudioClip cowDieClip;
    [Header("玩家死亡")] public AudioClip playerDieClip;
    [Header("怪物死亡")] public AudioClip enemyDieClip;
    
    
    // 音效播放器预制体（仅包含AudioSource组件）
    public GameObject audioPlayerPrefab;
    
    // 单例实例
    public static SoundAudioPool Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
        
        // 预加载 5个音效播放器（可根据需求调整）
        ObjectPool.SetPreload(audioPlayerPrefab, 8);
    }
    
    // 全局音量系数（ 0~1 ）
    private float globalSoundVolume = 1;
    // 保存音量大小
    [HideInInspector] public float previousVolume = 1;

    public float GlobalSoundVolume
    {
        get => globalSoundVolume;
        set
        {
            globalSoundVolume = Mathf.Clamp01(value); // 限制音量范围在 0~1 之间
            UpdateAllPlayingSoundVolume(); // 如果有正在播放的音效，实时更新它们的音量
        }
    }

    /// <summary>
    /// 实时更新正在播放的音效的音量
    /// </summary>
    private void UpdateAllPlayingSoundVolume()
    {
        PooledObject[] objects = FindObjectsOfType<PooledObject>();
        foreach (var player in objects)
        {
            float volume = player.GetComponent<AudioSource>().volume * globalSoundVolume;
            player.GetComponent<AudioSource>().volume = volume;
        }
    }


    /// <summary>
    /// 从池获取音效播放器并播放指定音效
    /// </summary>
    /// <param name="clip">音效片段</param>
    /// <param name="position">播放位置</param>
    /// <param name="volume">基础音量（0~1）</param>
    /// <param name="pitch">音调（默认1）</param>
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        // 从对象池获取播放器
        var audioPlayer = ObjectPool.Get(audioPlayerPrefab, position, Quaternion.identity);
        var audioSource = audioPlayer.GetComponent<AudioSource>();
        float soundVolume = globalSoundVolume * volume;
        
        // 设置音效属性
        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.pitch = pitch;
        audioSource.Play();
        
        // 播放完毕后自动回收
        float clipLength = clip.length;
        audioPlayer.ReturnAfter(clipLength);
    }
}