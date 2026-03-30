using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 旋转模型方法  
/// </summary>
public class ModelRotate : MonoBehaviour,IDragHandler
{
    [Header("旋转控制")]
    public GameObject targetModel; // 要旋转的模型容器
    public float rotateSpeed;
    private float targetYRotation;
    public float currentRotationY;
    public float smoothness;  // 平滑度，0=无平滑，越大越平滑


    public void OnDrag(PointerEventData eventData)
    {
        if (targetModel == null) return;

        // 根据鼠标拖拽旋转模型
        float rotateY = -eventData.delta.x * rotateSpeed * 0.01f;
        targetYRotation += rotateY;
    }

    private void Update()
    {
        if (targetModel == null) return;
        currentRotationY = Mathf.Lerp(currentRotationY, targetYRotation, Time.deltaTime * smoothness);
        // 应用旋转（只绕Y轴）
        targetModel.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }

    // 重置旋转
    public void ResetRotation()
    {
        if (targetModel != null)
        {
            currentRotationY = 0f;
            targetYRotation = 0f;
            targetModel.transform.rotation = Quaternion.identity;
        }
    }
    
    
}
