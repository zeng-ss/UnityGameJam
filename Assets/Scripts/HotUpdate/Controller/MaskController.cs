using UnityEngine;

public class MaskController : MonoBehaviour
{
    private float speed; // 每秒 xx度
    private float shotTimer;
    private float currentCoolTime; // 当前使用的冷却时间
    private string ParabolicBulletPoolName = "ParabolicBullet";
    private void Start()
    {
        currentCoolTime = Random.Range(3, 8);
    }

    private void Update()
    {
        if (!GameManager.Instance.RuntimeEnemyConfigDict.ContainsKey("Mask")) return;
        speed = GameManager.Instance.RuntimeEnemyConfigDict["Mask"].moveSpeed;
        // 围绕世界原点(0,0,0)旋转
        transform.RotateAround(Vector3.zero, Vector3.up, speed * Time.deltaTime);
        transform.LookAt(new Vector3(0, transform.position.y, 0));
        
        shotTimer += Time.deltaTime;
        if (shotTimer >= currentCoolTime)
        {
            GameObject obj = PoolMgr.Instance.GetObj(ParabolicBulletPoolName);
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.parabolicClip, transform.position);
            obj.transform.position = transform.position;
            // 1. 精准赋值发射旋转（朝向世界原点，和面具保持一致）
            obj.transform.rotation = Quaternion.LookRotation(transform.position - new Vector3(0, transform.position.y, 0), Vector3.up);
            // 2. 主动调用子弹的初始化方法（避免OnEnable时机问题）
            obj.GetComponent<ParabolicBullet>().InitBullet(transform.position);
            currentCoolTime = Random.Range(3, 8);
            shotTimer = 0;
        }
    }
}
