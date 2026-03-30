using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TipPanel : BasePanel
{
    public TMP_Text tipText;
    [FormerlySerializedAs("canvasGroup")] public CanvasGroup canvasGroup1;
    public float showTime = 1f;
    private float fadeDuration = 0.6f;
    private RectTransform rectTransform;

    private void OnEnable()
    {
        if (showTime >= 3) { showTime = 1; }
    }
    
    public override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup1 = GetComponent<CanvasGroup>();
    }

    public void ShowTip(string content)
    {
        tipText.text = content;
        canvasGroup1.alpha = 0f;
        
        // 动画序列
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup1.DOFade(1f, fadeDuration))
            .AppendInterval(showTime)
            .Append(canvasGroup1.DOFade(0f, fadeDuration))
            .OnComplete(() => UIManager.Instance.closePanel<TipPanel>());
    }
}
