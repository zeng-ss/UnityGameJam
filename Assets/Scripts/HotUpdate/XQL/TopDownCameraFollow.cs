using UnityEngine;

/// <summary>
/// 俯视角3D摄像机控制脚本
/// 挂载在主摄像机上，功能：跟随玩家+鼠标滚轮调节高度+边界限制+平滑过渡
/// </summary>
[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [Header("【核心跟随配置】")]
    [Tooltip("拖拽场景中的Player根节点（必须赋值）")]
    public Transform playerTarget;
    [Tooltip("摄像机与玩家的X/Z轴偏移（默认0,0 即始终在玩家正上方，俯视角建议不修改）")]
    public Vector3 xzOffset = new Vector3(0, 0, 0);
    [Tooltip("摄像机跟随平滑系数（值越大越流畅，俯视角建议0.15-0.3）")]
    public float followSmoothSpeed = 0.2f;

    [Header("【滚轮高度配置】")]
    [Tooltip("滚轮调节高度的灵敏度（值越大高度变化越快，俯视角建议0.3-0.8）")]
    public float scrollHeightSensitivity = 0.5f;
    [Tooltip("摄像机最低高度（避免过低穿模，俯视角建议3-5）")]
    public float minCameraHeight = 4f;
    [Tooltip("摄像机最高高度（避免过高丢失视野，俯视角建议8-15）")]
    public float maxCameraHeight = 10f;
    [Tooltip("高度调节平滑系数（与跟随平滑一致即可，避免高度突变）")]
    public float heightSmoothSpeed = 0.2f;

    [Header("【边界限制配置】")]
    [Tooltip("是否限制摄像机跟随范围（防止玩家走出地图时摄像机脱节，建议开启）")]
    public bool limitFollowArea = true;
    [Tooltip("摄像机X轴跟随范围（左右）")]
    public Vector2 followRangeX = new Vector2(-20f, 20f);
    [Tooltip("摄像机Z轴跟随范围（前后）")]
    public Vector2 followRangeZ = new Vector2(-20f, 20f);

    // 私有变量：当前目标高度、目标位置
    private float _targetHeight;
    private Vector3 _targetFollowPos;
    private Camera _mainCamera;

    private void Awake()
    {
        // 自动获取主相机组件
        _mainCamera = GetComponent<Camera>();
        // 校验玩家目标是否赋值
        if (playerTarget == null)
        {
            Debug.LogError("未指定玩家目标！请在Inspector面板拖拽Player根节点到playerTarget字段！");
            enabled = false;
            return;
        }
        // 初始化目标高度为摄像机当前高度（保持开局视角不变）
        _targetHeight = transform.position.y;
        // 初始化跟随目标位置
        _targetFollowPos = GetLimitFollowPos();
    }

    private void LateUpdate()
    {
        if (playerTarget == null) return;

        // 1. 处理鼠标滚轮输入，更新目标高度
        HandleScrollWheelInput();
        // 2. 计算带边界的玩家跟随位置
        _targetFollowPos = GetLimitFollowPos();
        // 3. 平滑更新摄像机位置（跟随+高度调节）
        SmoothUpdateCameraPos();
    }

    /// <summary>
    /// 处理鼠标滚轮输入，更新目标高度（带边界限制）
    /// </summary>
    private void HandleScrollWheelInput()
    {
        // 获取滚轮输入：向前滚动为正（升高），向后为负（降低）
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollValue) < 0.001f) return;

        // 更新目标高度
        _targetHeight += scrollValue * scrollHeightSensitivity * 10;
        // 限制高度在最小/最大值之间
        _targetHeight = Mathf.Clamp(_targetHeight, minCameraHeight, maxCameraHeight);
    }

    /// <summary>
    /// 计算带边界的跟随位置（X/Z轴）
    /// </summary>
    private Vector3 GetLimitFollowPos()
    {
        Vector3 followPos = playerTarget.position + xzOffset;
        // 若开启跟随范围限制，对X/Z轴做边界裁剪
        if (limitFollowArea)
        {
            followPos.x = Mathf.Clamp(followPos.x, followRangeX.x, followRangeX.y);
            followPos.z = Mathf.Clamp(followPos.z, followRangeZ.x, followRangeZ.y);
        }
        // 始终保持Y轴为目标高度（俯视角核心）
        followPos.y = _targetHeight;
        return followPos;
    }

    /// <summary>
    /// 平滑更新摄像机位置（跟随+高度调节一体化）
    /// </summary>
    private void SmoothUpdateCameraPos()
    {
        // 平滑插值计算最终位置，LateUpdate执行避免与玩家移动逻辑冲突
        Vector3 smoothPos = Vector3.Lerp(transform.position, _targetFollowPos, followSmoothSpeed * Time.deltaTime * 60);
        // 赋值摄像机位置，保持俯视角
        transform.position = smoothPos;
        // 摄像机始终垂直向下看（纯俯视角，固定旋转，不随玩家改变）
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // 可选：Gizmos绘制跟随范围，便于场景中调试
    private void OnDrawGizmosSelected()
    {
        if (limitFollowArea && playerTarget != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((followRangeX.x + followRangeX.y) / 2, playerTarget.position.y, (followRangeZ.x + followRangeZ.y) / 2);
            Vector3 size = new Vector3(followRangeX.y - followRangeX.x, 0.1f, followRangeZ.y - followRangeZ.x);
            Gizmos.DrawWireCube(center, size);
        }
    }
}