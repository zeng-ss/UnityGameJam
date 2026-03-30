using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 按键绑定管理器
/// </summary>
public class KeyBindingManager : UnitySingleTonMono<KeyBindingManager>
{
    // 按键绑定配置（单按键）
    public Dictionary<GameAction, KeyBinding> keyBindingsDict = new ();
    // 是否打开设置面板，打开就禁止输入等待面板关闭
    [HideInInspector] public bool isEscKey;
    void Start() { InitDefaultBindings(); }

    /// <summary>
    /// 初始化默认按键映射
    /// </summary>
    public void InitDefaultBindings()
    {
        keyBindingsDict.Clear();
        UpdateBinding(GameAction.Crouch,KeyCode.C,"下蹲");
        UpdateBinding(GameAction.PickUp, KeyCode.E,"捡起");
        UpdateBinding(GameAction.Attack, KeyCode.J,"攻击");
        UpdateBinding(GameAction.Drop,KeyCode.G,"丢弃道具");
        UpdateBinding(GameAction.Jump, KeyCode.Space,"跳跃");
        UpdateBinding(GameAction.Interact, KeyCode.F,"交互");
        UpdateBinding(GameAction.UsingItem, KeyCode.Mouse1,"使用道具");
    }

    /// <summary>
    /// 重置按键映射为默认
    /// </summary>
    public void ResetBindings()
    {
        keyBindingsDict.Clear();
        UpdateBinding(GameAction.Crouch,KeyCode.C,"下蹲");
        UpdateBinding(GameAction.PickUp, KeyCode.E,"捡起");
        UpdateBinding(GameAction.Attack, KeyCode.J,"攻击");
        UpdateBinding(GameAction.Drop,KeyCode.G,"丢弃道具");
        UpdateBinding(GameAction.Jump, KeyCode.Space,"跳跃");
        UpdateBinding(GameAction.Interact, KeyCode.F,"交互");
        UpdateBinding(GameAction.UsingItem, KeyCode.Mouse1,"使用道具");
    }

    /// <summary>
    /// 更新单按键绑定
    /// </summary>
    public void UpdateBinding(GameAction action, KeyCode keyCode, string actionName = "未知")
    {
        if (!keyBindingsDict.ContainsKey(action))
        {
            // keyBindingsDict[action] = new KeyBinding(action, keyCode) 和 
            // keyBindingsDict.Add(action,new KeyBinding(action, keyCode))等价
            
            //keyBindingsDict.Add(action,new KeyBinding(action, keyCode));
            keyBindingsDict[action] = new KeyBinding(action, keyCode, actionName);
        }
        else
        {
            // 检测新按键是否已被其他操作绑定
            foreach (var kv in keyBindingsDict)
            {
                if (kv.Key != action && kv.Value.bindedKey == keyCode)
                {
                    //提示玩家（弹出 UI 提示）
                    UIManager.Instance.openPanel<TipPanel>(tipPanel =>
                    {
                        tipPanel.ShowTip($"按键 {keyCode} 已被 {kv.Value.actionName} 绑定！");
                        tipPanel.transform.localPosition = new Vector2(0, 430);
                    });
                    return; // 拒绝绑定，避免冲突
                }
            }
            keyBindingsDict[action].bindedKey = keyCode;
        }
    }
    
    /// <summary>
    /// 检测操作是否被触发（按下）
    /// </summary>
    public bool GetActionDown(GameAction action)
    {
        if (!keyBindingsDict.ContainsKey(action)) return false;
        return Input.GetKeyDown(keyBindingsDict[action].bindedKey);
    }
    
    /// <summary>
    /// 检测操作是否持续触发（按住）
    /// </summary>
    public bool GetAction(GameAction action)
    {
        if (!keyBindingsDict.ContainsKey(action)) return false;
        return Input.GetKey(keyBindingsDict[action].bindedKey);
    }

    /// <summary>
    /// 获取操作当前绑定的按键（用于面板显示）
    /// </summary>
    public KeyCode GetBindedKey(GameAction action)
    {
        return keyBindingsDict.ContainsKey(action) ? keyBindingsDict[action].bindedKey : KeyCode.None;
    }
    
}
