using System.Collections;
using UnityEngine;


public class WeaponController : MonoBehaviour
{
    public string curEnemyName; // 当前武器持有者的名字
    private string HitPlayerBloodPoolName = "HitPlayerBlood";
    [Header("武器碰撞器")] [SerializeField] private new Collider collider;
    private bool isFirstStay = true;
    
    // 武器 技能释放开始事件函数
    public void StartAttackHit()
    {
        //打开碰撞器开始检测
        collider.enabled = true;
        isFirstStay = true;
    }
    // 武器 技能释放结束事件函数
    public void StopAttackHit()
    {
        collider.enabled = false;
        isFirstStay = false;
    }
    
    /// <summary>
    /// 检测武器和敌人之间的碰撞
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)//持续检测碰撞效果
    {
        //检测当前武器所能
        if (other.CompareTag("Player"))
        {
            PlayerStats player =  other.gameObject.GetComponent<PlayerStats>();//拿到被攻击的对象
            if (player != null && isFirstStay)
            {
                isFirstStay = false;
                GameObject blood = PoolMgr.Instance.GetObj(HitPlayerBloodPoolName);
                blood.transform.position = other.ClosestPoint(transform.position);
                float decreaseHealth = GameManager.Instance.RuntimeEnemyConfigDict[curEnemyName].damage;
                SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.hitPlayerClip,transform.position);
                player.TakeDamage(decreaseHealth);
                // 玩家扣血
                StartCoroutine(PushBlood(blood));
            }
        }
    }

    private IEnumerator PushBlood(GameObject blood)
    {
        yield return new WaitForSeconds(blood.GetComponent<ParticleSystem>().main.duration);
        PoolMgr.Instance.PushObj(HitPlayerBloodPoolName,blood);
    }
    
}
