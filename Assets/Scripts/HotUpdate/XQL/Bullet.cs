using UnityEngine;

// 子弹核心脚本：挂载到ZiDan预制体，负责直线飞行、射程检测、超射程销毁
public class Bullet : MonoBehaviour
{
    private float _bulletSpeed; // 子弹飞行速度
    private float _maxRange; // 子弹最大射程
    private float damanage;
    private Vector3 _shootDirection; // 子弹飞行方向
    private Vector3 _spawnPosition; // 子弹生成位置（用于计算飞行距离）

    /// <summary>
    /// 初始化子弹参数（由PlayerController调用，传递核心数值）
    /// </summary>
    /// <param name="speed">飞行速度</param>
    /// <param name="range">最大射程</param>
    /// <param name="direction">飞行方向</param>
    public void InitBullet(float speed, float range,float damage ,Vector3 direction)
    {
        _bulletSpeed = speed;
        _maxRange = range;
        damanage = damage;
        _shootDirection = direction.normalized; // 确保方向归一化
        _spawnPosition = transform.position; // 记录生成位置

        // 校验参数，避免异常
        if (_bulletSpeed <= 0) _bulletSpeed = 20f;
        if (_maxRange <= 0) _maxRange = 15f;
    }

    private void Update()
    {
        // 子弹直线飞行（无重力，沿指定方向匀速移动）
        transform.Translate(_shootDirection * (_bulletSpeed * Time.deltaTime), Space.World);

        // 检测是否超出射程
        CheckBulletRange();
    }

    /// <summary>
    /// 检测子弹是否超出最大射程，超出则销毁
    /// </summary>
    private void CheckBulletRange()
    {
        float currentDistance = Vector3.Distance(transform.position, _spawnPosition);
        if (currentDistance >= _maxRange)
        {
            Destroy(gameObject); // 超出射程，销毁子弹
            // Debug.Log($"子弹超出射程（{_maxRange}m），已销毁");
        }
    }

    // 预留：子弹碰撞检测（后续添加击中效果时使用）
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject.GetComponent<EnemyController>() != null) 
            {
                other.gameObject.GetComponent<EnemyController>().DecreaseHealth(damanage);
            }
            else
            {
                other.gameObject.GetComponent<RobotController>().DecreaseHealth(damanage);
            }
            Destroy(gameObject);
        }
    }
}