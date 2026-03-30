using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class BasePanel : MonoBehaviour
{
    // 新增：存储面板预制体的原始RectTransform属性，解决二次打开位置错位
    [HideInInspector] public Vector3 originAnchoredPos;
    [HideInInspector] public Vector2 originAnchorMin;
    [HideInInspector] public Vector2 originAnchorMax;
    [HideInInspector] public Vector2 originSizeDelta;
    [HideInInspector] public Vector2 originPivot;
    
    protected CanvasGroup canvasGroup;
    
    private RectTransform rootRect;
    protected IEnumerator DelayedLayoutUpdate()
    {
        yield return null; // 等待一帧
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
    }
    //淡入淡出速度 
    private bool isShow;
    private UnityAction hideAction;
    public virtual void Awake()
    {
        rootRect = this.transform as RectTransform;
        canvasGroup = GameObject.Find("Canvas")?.GetComponent<CanvasGroup>();
        // canvasGroup = GetComponent<CanvasGroup>();
        // if (canvasGroup == null) canvasGroup= this.gameObject.AddComponent<CanvasGroup>();
    }

    public virtual void Show()//虚函数 能够被重写  
    {
        this.gameObject.SetActive(true);
    }


    public virtual void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
