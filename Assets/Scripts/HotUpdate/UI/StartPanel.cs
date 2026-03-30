using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    public Button startBtn;
    public TMP_Text startText;
    public Button endlessBtn;
    public TMP_Text endlessText;
    public Button exitBtn;
    public TMP_Text exitText;

    private float timer;
    
    private void OnEnable()
    {
        startBtn.onClick.AddListener(StartClick);
        endlessBtn.onClick.AddListener(EndlessBtnClick);
        exitBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.btnSeletClip, transform.position);
            // 退出游戏
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;    
#else
            Application.Quit();
#endif
        });
        // 添加鼠标悬停事件
        AddHoverEvents();
    }

    private void StartClick()
    {
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.btnSeletClip, transform.position);
        UIManager.Instance.openPanel<TipPanel>(panel =>
        {
            panel.ShowTip("敬请期待....");
        });
    }

    private void EndlessBtnClick()
    {
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.btnSeletClip, transform.position);
        SceneMgr.Instance.LoadSceneAsync("EndlessScene", () =>
        {
            UIManager.Instance.closePanel<StartPanel>();
            UIManager.Instance.openPanel<PlayerProPanel>();
        });
    }

    // 添加悬停事件
    private void AddHoverEvents()
    {
        AddHoverToButton(startBtn);
        AddHoverToButton(endlessBtn);
        AddHoverToButton(exitBtn);
    }

    private void Update()
    {
        timer += Time.deltaTime * 2;
        startText.fontSize = Mathf.Lerp(30, 50, (Mathf.Sin(timer) + 1) * 0.5f);
        endlessText.fontSize = Mathf.Lerp(30, 50, (Mathf.Sin(timer) + 1) * 0.5f);
        exitText.fontSize = Mathf.Lerp(30, 50, (Mathf.Sin(timer) + 1) * 0.5f);
        
    }
    
    private void AddHoverToButton(Button button)
    {
        // 使用EventTrigger
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();
        // 鼠标进入事件
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => 
        {
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.hoverClip, transform.position);
        });
        trigger.triggers.Add(entryEnter);
    }

    private void OnDisable()
    {
        startBtn.onClick.RemoveAllListeners();
        endlessBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
    }
    
    
    
}
