using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class TimerPanel : BasePanel
{
    public TMP_Text timerText;
    private Coroutine countdownCoroutine;
    private float currentTime;
    private bool isStartplaySound;
    
    // 开始倒计时
    public void StartCountdown(float totalTime)
    {
        // 如果已经在倒计时，先停止
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        currentTime = totalTime;
        isStartplaySound = false;
        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }
    
    // 协程倒计时
    private IEnumerator CountdownRoutine()
    {
        while (currentTime > 0)
        {
            UpdateDisplay();
            yield return null; // 每帧更新
            currentTime -= Time.deltaTime;
        }
        
        // 倒计时结束
        currentTime = 0;
        UpdateDisplay();
        OnCountdownFinished();
    }
    
    // 更新显示
    private void UpdateDisplay()
    {
        // 格式化为 00:00 或 0
        timerText.text = $"下一波敌人倒计时：{FormatTime(currentTime)}";
        // 最后闪烁效果
        if (currentTime <= 3f)
        {
            if (!isStartplaySound)
            {
                SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.warningClip, transform.position);
                isStartplaySound = true;
            }
            timerText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 5, 1));
        }
    }
    
    // 格式化时间
    private string FormatTime(float time)
    {
        // 方法1：纯数字
        //return Mathf.Ceil(time).ToString();
        // 方法2：分钟:秒钟
        //int minutes = Mathf.FloorToInt(time / 60);
        //int seconds = Mathf.FloorToInt(time % 60);
        //return $"{minutes:00}:{seconds:00}";
        // 方法3：带小数点
        return time.ToString("F1");
    }
    
    // 倒计时结束回调
    private void OnCountdownFinished()
    {
        transform.DOLocalMoveY(112, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            UIManager.Instance.closePanel<TimerPanel>();
        });
        // 可以触发事件
        // EventManager.Instance.TriggerEvent("CountdownFinished");
        
        // 或者调用回调
        // OnTimerEnd?.Invoke();
    }
    
    // 暂停倒计时
    public void PauseCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
    }
    
    // 继续倒计时
    public void ResumeCountdown()
    {
        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }
    
    // 停止倒计时
    public void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }
}