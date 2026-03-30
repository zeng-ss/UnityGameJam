using UnityEngine;

public class EnemyBulletController : MonoBehaviour
{
    private float hitBloodValue;
    private string bulletType;
    private float timer;

    private void Update()
    {
        // 移动子弹
        transform.Translate(Vector3.forward * (20 * Time.deltaTime), Space.Self);
        timer += Time.deltaTime;
        // 发射后5秒回收
        if (timer >= 5f)
        {
            PoolMgr.Instance.PushObj(bulletType, gameObject);
            timer = 0;
        }
    }

    public void Init(string bulletType,float hit)
    {
        hitBloodValue = hit;
        this.bulletType = bulletType;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.hitPlayerClip,transform.position);
        // 玩家扣血
        other.gameObject.GetComponent<PlayerStats>().TakeDamage(hitBloodValue);
        // 回收子弹
        PoolMgr.Instance.PushObj(bulletType, gameObject);
    }
    
}
