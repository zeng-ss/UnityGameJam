using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EscPanel : BasePanel
{
    public Button backBtn;
    public Button pauseBtn;
    public Button closeBtn;
    
    private void Start()
    {
        backBtn.onClick.AddListener(BackClick);
        pauseBtn.onClick.AddListener(pauseBtnOnClick);
        
        closeBtn.onClick.AddListener(() =>
        {
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.panelHideClip, transform.position);
            transform.DOLocalMove(new Vector3(0,751,0), 0.5f)
                .OnComplete(() => {UIManager.Instance.closePanel<EscPanel>();});
        });
        // 添加鼠标悬停事件
        AddHoverEvents();
    }

    private void pauseBtnOnClick()
    {
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.btnSeletClip, transform.position);
        TMP_Text pauseText = pauseBtn.GetComponentInChildren<TMP_Text>();
        if (pauseText.text == "暂停")
        {
            Time.timeScale = 0;
            pauseText.text = "继续";
        }
        else
        {
            Time.timeScale = 1;
            pauseText.text = "暂停";
        }
    }

    private void BackClick()
    {
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.btnSeletClip, transform.position);
        SceneMgr.Instance.LoadSceneAsync("StartScene", () =>
        {
            PoolMgr.Instance.Clear();
            MaskSystemManager.Instance.IsMaskSelected = false;
            UIManager.Instance.closePanel<EscPanel>();
            UIManager.Instance.closePanel<PlayerProPanel>();
        });
    }

    // 添加悬停事件
    private void AddHoverEvents()
    {
        AddHoverToButton(backBtn);
        AddHoverToButton(pauseBtn);
        AddHoverToButton(closeBtn);
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

    private void OnDestroy()
    {
        backBtn.onClick.RemoveAllListeners();
        pauseBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.RemoveAllListeners();
    }
    
}
