using System;
using UnityEngine;
using DG.Tweening;

public class RobotController : MonoBehaviour
{
    private GameObject player;
    
    public GameObject fireEffect;
    private string SingleBulletPrefabName = "SingleBullet";
    private string MultiBulletPrefabName = "MultiBullet";
    private float bulletCooldownTimer; // 计时器
    private float bulletCooldownTime;  // 冷却时间
    private float hitBlood; //击中玩家扣的血量
    private EnemyConfigRuntime currentEnemyData;
    private float currentHealth;
    
    private float gravity = -9.8f;
    private Vector3 volecity;
    private CharacterController characterController;
    [Header("是否在地面")] private bool isOnGround;
    public SphereCollider sphereCollider;


    public enum RobotType
    {
        Single,
        Multi
    }
    public RobotType robotType;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (!GameManager.Instance.RuntimeEnemyConfigDict.ContainsKey(gameObject.name)) return;
        currentEnemyData = GameManager.Instance.RuntimeEnemyConfigDict[gameObject.name];
        hitBlood = currentEnemyData.damage;
        currentHealth = currentEnemyData.health;
        characterController.enabled = true;
        sphereCollider.enabled = true;
    }

    private void Start()
    {
        player = SceneObjectManager.Instance.GetObjectByTag("Player");
    }
    
    private void Update()
    {
        if (currentEnemyData == null) return;
        if (characterController.enabled)
        {
            characterController.Move(volecity * Time.deltaTime);//先移动
            //判断是否在地面
            isOnGround = characterController.isGrounded;
            if (isOnGround) { volecity.y = -2f; }
            else { volecity.y += gravity * Time.deltaTime; }
        }
        // 计算到玩家的方向
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0; // 保持水平方向
        transform.rotation = Quaternion.LookRotation(-direction);
        ShotBullet();
    }

    private void ShotBullet()
    {
        if (player == null) return;
        bulletCooldownTimer += Time.deltaTime;
        switch (robotType)
        {
            case RobotType.Single:
                bulletCooldownTime = 2f;
                if (bulletCooldownTimer >= bulletCooldownTime)
                {
                    GameObject obj = PoolMgr.Instance.GetObj(SingleBulletPrefabName);
                    SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.singleFireClip, transform.position,0.2f);
                    obj.transform.position = fireEffect.transform.position;
                    obj.transform.forward = -transform.forward;
                    obj.GetComponent<EnemyBulletController>().Init(SingleBulletPrefabName,hitBlood);
                    bulletCooldownTimer = 0;
                }
                break;
            case RobotType.Multi:
                bulletCooldownTime = 0.6f;
                if (bulletCooldownTimer >= bulletCooldownTime)
                {
                    GameObject obj = PoolMgr.Instance.GetObj(MultiBulletPrefabName);
                    SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.multiFireClip, transform.position,0.4f);
                    obj.transform.position = fireEffect.transform.position;
                    obj.transform.forward = -transform.forward;
                    obj.GetComponent<EnemyBulletController>().Init(MultiBulletPrefabName,hitBlood);
                    bulletCooldownTimer = 0;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;
        if (!(currentHealth <= 0)) return;
        sphereCollider.enabled = false;
        characterController.enabled = false;
        // 死亡
        // 克隆爆炸效果
        GameObject explo = PoolMgr.Instance.GetObj("Explo");
        explo.transform.position = transform.position;
        SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.robotExploClip, transform.position);
        // 缩放震动 + 消失
        Sequence deathSeq = DOTween.Sequence();
        // 缩放震动（强烈）
        deathSeq.Append(transform.DOShakeScale(0.5f, 0.8f, 20));
        // 同时旋转
        deathSeq.Join(transform.DORotate(new Vector3(0, 720, 0), 0.8f, RotateMode.FastBeyond360));
        // 上抛然后掉落（模拟死亡弹跳）
        deathSeq.Join(transform.DOJump(transform.position + Vector3.up * 4, 1f, 1, 0.8f));
        // 最后缩小到 0
        deathSeq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        // 完成
        deathSeq.OnComplete(() =>
        {
            PoolMgr.Instance.PushObj(gameObject.name, gameObject);
            PoolMgr.Instance.PushObj("Explo", explo);
        });
    }
    
}
