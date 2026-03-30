using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private string cowPoolName = "Cow";
    private string StarFishPoolName = "StarFish";
    private string BlueSlimePoolName = "BlueSlime";
    private string TurtleShellPoolName = "TurtleShell";

    public WeaponController weapon;
    private GameObject player;
    private EnemyConfigRuntime currentEnemyData;
    private float currentHealth;
    private bool isDead;
    private bool isMoving;
    private bool isAttacking;
    private float distance;

    private float gravity = -9.8f;
    private Vector3 volecity;
    private Animator animator;
    private CharacterController characterController;
    public SphereCollider sphereCollider;
    public CapsuleCollider capsuleCollider;
    [Header("是否在地面")] private bool isOnGround;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        characterController.enabled = true;
        if (sphereCollider != null) { sphereCollider.enabled = true; }
        else { capsuleCollider.enabled = true; }
        isDead = false;
        isMoving = false;
        isAttacking = false;
        if (!GameManager.Instance.RuntimeEnemyConfigDict.ContainsKey(gameObject.name)) return;
        currentEnemyData = GameManager.Instance.RuntimeEnemyConfigDict[gameObject.name];
        currentHealth = currentEnemyData.health;
        StartCoroutine(StartMoveToPlayer());
    }

    private void Start()
    {
        player = SceneObjectManager.Instance.GetObjectByTag("Player");
    }

    private IEnumerator StartMoveToPlayer()
    {
        yield return new WaitForSeconds(1f);
        if (player == null) yield break;
        animator.SetBool("IsMove", true);
        isMoving = true;
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
        if (player == null) return;
        if (isDead) { HandleDeath(); return; }
        distance = Vector3.Distance(player.transform.position, transform.position);
        if (isMoving) { HandleMove(); return; }
        if (isAttacking) { HandleAttack(); }
    }

    #region 动画相关
    private void HandleDeath()
    {
        isAttacking = false;
        isMoving = false;
        animator.SetBool("IsDie", true);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Die") && stateInfo.normalizedTime >= 1f)
        {
            switch (gameObject.name)
            {
                case "Cow":
                    PoolMgr.Instance.PushObj(cowPoolName, gameObject);
                    break;
                case "StarFish":
                    PoolMgr.Instance.PushObj(StarFishPoolName, gameObject);
                    break;
                case "TurtleShell":
                    PoolMgr.Instance.PushObj(TurtleShellPoolName, gameObject);
                    break;
                case "BlueSlime":
                    PoolMgr.Instance.PushObj(BlueSlimePoolName, gameObject);
                    break;
            }
        }
    }
    private void HandleMove()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0; // Y轴不动
        // 面向玩家（只旋转Y轴）
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        // CharacterController 移动
        float currentMoveSpeed = currentEnemyData.moveSpeed;
        characterController.Move(direction.normalized * (currentMoveSpeed * Time.deltaTime));
        if (distance <= 2.5f)
        {
            isMoving = false;
            animator.SetBool("IsAttack", true);
            isAttacking = true;
        }
    }
    private void HandleAttack()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 0.9f && distance > 1)
        {
            isAttacking = false;
            isMoving = true;
            animator.SetBool("IsAttack", false);
        }
    }
    
    #endregion

    public void StartAttackHit() { weapon.StartAttackHit(); }
    public void StopAttackHit() { weapon.StopAttackHit(); }
    
    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;
        if (!(currentHealth <= 0)) return;
        characterController.enabled = false;
        if (sphereCollider != null) { sphereCollider.enabled = false; }
        else { capsuleCollider.enabled = false; }
        if (gameObject.name == "Cow")
        {
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.cowDieClip, transform.position);
        }
        else
        {
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.enemyDieClip, transform.position,0.5f);
        }
        isDead = true;
        isMoving = false;
        isAttacking = false;
    }
    
}
