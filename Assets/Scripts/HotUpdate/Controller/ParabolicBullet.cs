using UnityEngine;

public class ParabolicBullet : MonoBehaviour
{
    private Vector3 startPoint;    // 起点（面具发射点）
    private Vector3 targetPoint;   // 终点（玩家位置）
    [Header("抛物线配置")]
    public float height = 5f;       // 抛物线高度
    public float moveSpeed = 10f;   // 移动速度（单位/秒，统一速度）
    private float progress;         // 进度 0-1

    private PlayerStats player;
    private GameObject redWarnTip;
    private string ExploPoolName = "Explo";
    private string RedWarnPoolName = "RedWarn";
    private string ParabolicBulletPoolName = "ParabolicBullet";
    private bool isInit;      // 是否完成初始化

    private float decreaseHealth;

    private void Start()
    {
        decreaseHealth = GameManager.Instance.RuntimeEnemyConfigDict["Mask"].damage;
        // 提前获取玩家引用，避免重复查找
        player = SceneObjectManager.Instance.GetObjectByTag<PlayerStats>("Player");
    }
    
    public void InitBullet(Vector3 spawnPos)
    {
        // 重置所有状态
        progress = 0f;
        isInit = false;
        startPoint = spawnPos;
        targetPoint = Vector3.zero;
        // 延迟1帧获取玩家位置（确保发射位置已完全赋值）
        Invoke(nameof(SetTargetPoint), 0.01f);
    }

    // 手动设置目标点（避免帧同步问题）
    private void SetTargetPoint()
    {
        if (player != null)
        {
            targetPoint = player.transform.position;
            // 确保目标点和起点在同一水平高度（避免Y轴偏差）
            targetPoint.y = -0.8f;
            redWarnTip = PoolMgr.Instance.GetObj(RedWarnPoolName);
            redWarnTip.transform.position = targetPoint;
            isInit = true;
        }
        else
        {
            // 无玩家则直接回收
            RecycleSelf();
        }
    }

    private void Update()
    {
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        // 未初始化完成则跳过
        if (!isInit || player == null) return;
        // 距离过近直接回收
        float distance = Vector3.Distance(startPoint, targetPoint);
        if (distance < 0.1f)
        {
            RecycleSelf();
            return;
        }
        // 计算进度（基于距离和速度，确保不同距离飞行速度一致）
        float step = moveSpeed * Time.deltaTime / distance;
        progress += step;
        progress = Mathf.Clamp01(progress);

        // 抛物线核心计算（修正版）
        float t = progress;
        // 标准抛物线公式：y = h * 4t(1-t)
        float currentHeight = height * 4 * t * (1 - t);
        // 水平插值（仅XZ轴）
        Vector3 horizontalPos = Vector3.Lerp(new Vector3(startPoint.x, 0, startPoint.z), 
            new Vector3(targetPoint.x, 0, targetPoint.z), t
        );
        // 最终位置（叠加高度）
        transform.position = new Vector3(horizontalPos.x, startPoint.y + currentHeight, horizontalPos.z);

        // 朝向飞行方向（可选，让子弹始终朝向目标）
        Vector3 nextPos = Vector3.Lerp(startPoint, targetPoint, Mathf.Clamp01(t + 0.05f));
        nextPos.y = startPoint.y + height * 4 * (t + 0.05f) * (1 - (t + 0.05f));
        transform.LookAt(nextPos);
        // 到达终点回收
        if (progress >= 1f)
        {
            Explode();
            RecycleSelf();
        }
    }

    private void Explode()
    {
        // 爆炸特效/伤害检测逻辑
        // 计算玩家到爆炸中心的距离
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.parabolicExploClip, redWarnTip.transform.position,0.3f);
        GameObject explo = PoolMgr.Instance.GetObj(ExploPoolName);
        explo.transform.position = redWarnTip.transform.position;
        float distance = Vector3.Distance(redWarnTip.transform.position, player.transform.position);
        // 检测是否在警示圈范围内
        if (distance <= 1.7f)
        {
            //print("玩家在爆炸范围内！距离：" + distance);
            // 玩家扣血 decreaseHealth
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.hitPlayerClip,transform.position);
            player.TakeDamage(decreaseHealth);
        }
    }

    // 统一回收方法
    private void RecycleSelf()
    {
        isInit = false;
        PoolMgr.Instance.PushObj(RedWarnPoolName, redWarnTip);
        PoolMgr.Instance.PushObj(ParabolicBulletPoolName, gameObject);
    }

    // 禁用时重置状态（防止残留）
    private void OnDisable()
    {
        progress = 0f;
        isInit = false;
        startPoint = Vector3.zero;
        targetPoint = Vector3.zero;
    }
    
}