using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    #region 池子数据
    
    private string CowPoolName = "Cow";
    private string ExploPoolName = "Explo";
    private string RedWarnPoolName = "RedWarn";
    private string StarFishPoolName = "StarFish";
    private string BlueSlimePoolName = "BlueSlime";
    private string MultiRobotPoolName = "MultiRobot";
    private string TurtleShellPoolName = "TurtleShell";
    private string SingleRobotPoolName = "SingleRobot";
    private string MultiBulletPoolName = "MultiBullet";
    private string SingleBulletPoolName = "SingleBullet";
    private string HitPlayerBloodPoolName = "HitPlayerBlood";
    private string ParabolicBulletPoolName = "ParabolicBullet";

    #endregion

    #region 配置

    [System.Serializable]
    public class EnemyType
    {
        public string prefabName;  // 预制体名称
        public int weight;         // 权重（越高越容易生成）
        public int initWeight;     // 初始权重（用于重置）
    }
    public List<EnemyType> enemyTypes = new();
    private float timer;
    private GameObject player;
    
    [Header("基础配置")]
    [Tooltip("初始每波生成数量")] public int initBatchSize = 5;
    [Tooltip("初始最大敌人数")] public int initMaxEnemies = 30;
    [Tooltip("初始生成间隔")] public float initSpawnInterval = 5f;
    [Tooltip("生成半径")] public float spawnRadius = 30;
    [Tooltip("同批次生成敌人的最小间距（单位）")] public float minEnemyDistance = 4f; // 默认为2单位

    [Header("难度递进配置")]
    [Tooltip("每波增加的最大敌人数")] public int waveAddMaxEnemies = 5;
    [Tooltip("每波减少的生成间隔（秒）")] public float waveReduceInterval = 0.2f;
    [Tooltip("生成间隔最低限制（秒）")] public float minSpawnInterval = 2f;
    [Tooltip("高权重敌人类型名称")] public List<string> highWeightEnemyNames;
    [Tooltip("每波给高权重敌人增加的权重")] public int waveAddHighWeight = 2;
    
    [Tooltip("雨列表")] public List<GameObject> rains;

    #endregion

    #region 运行时变量

    private int currentWave = 1;          // 当前波次
    private int currentBatchSize;         // 当前每波生成数量
    private int currentMaxEnemies;        // 当前最大敌人数
    private float currentSpawnInterval;   // 当前生成间隔
    private bool isWaitingForClear;       // 是否等待清场（当前波次敌人已生成完毕）
    private int currentWaveSpawnedCount;  // 当前波次已生成的敌人数（核心控制）

    #endregion
    
    private void Start()
    {
        MaskSelectUIManager.Instance.ShowMaskSelectPanel();
        // 初始化玩家引用
        player = SceneObjectManager.Instance.GetObjectByTag("Player");
        // 初始化难度参数
        currentBatchSize = initBatchSize;
        currentMaxEnemies = initMaxEnemies;
        currentSpawnInterval = initSpawnInterval;
        // 记录敌人初始权重（用于难度递增时修改）
        foreach (var type in enemyTypes)
        {
            type.initWeight = type.weight;
        }
        // 预加载对象池
        PreloadAllPools();
        UIManager.Instance.openPanel<TimerPanel>(timerPanel =>
        {
            timerPanel.StartCountdown(currentSpawnInterval);
            timerPanel.transform.DOLocalMoveY(112, 0.3f).SetEase(Ease.OutBack);
        });
    }

    private void Update()
    {
        // 玩家为空则直接返回
        if (player == null) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemys)
            {
                if (enemy.GetComponent<EnemyController>() != null)
                {
                    enemy.GetComponent<EnemyController>().DecreaseHealth(100);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemys)
            {
                if (enemy.GetComponent<EnemyController>() == null)
                {
                    enemy.GetComponent<RobotController>().DecreaseHealth(100);
                }
            }
        }
        // 检测是否清完所有敌人（触发波次升级）
        CheckWaveClear();
        // 仅当 非等待清场 + 未生成完当前波次总数量 时，才计时生成
        if (!isWaitingForClear && currentWaveSpawnedCount < currentMaxEnemies)
        {
            timer += Time.deltaTime;
            if (timer >= currentSpawnInterval)
            {
                SpawnEnemyBatch();
                timer = 0;
            }
        }
    }

    #region 波次检测与难度升级
    
    /// <summary>
    /// 检测当前波次是否清完，清完则升级波次
    /// </summary>
    private void CheckWaveClear()
    {
        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        // 当前波次已生成完毕（达到最大敌人），且敌人已清完 → 升级波次
        if (isWaitingForClear && currentEnemyCount == 0)
        {
            UpgradeWave();
            return;
        }
        // 当前波次未生成完毕，但已达到最大敌人 → 标记为等待清场
        if (!isWaitingForClear && currentWaveSpawnedCount >= currentMaxEnemies)
        {
            isWaitingForClear = true;
        }
    }

    /// <summary>
    /// 升级波次，提升难度
    /// </summary>
    private void UpgradeWave()
    {
        // 波次+1
        currentWave++;
        PatternSelectUIManager.Instance.OnWaveEnemyDestroyed();
        RandomStartRain();
        // 提升难度：增加最大敌人数
        currentMaxEnemies += waveAddMaxEnemies;
        // 提升难度：缩短生成间隔（不低于最小值）
        currentSpawnInterval = Mathf.Max(currentSpawnInterval - waveReduceInterval, minSpawnInterval);
        UIManager.Instance.openPanel<TimerPanel>((timerPanel =>
        {
            timerPanel.StartCountdown(currentSpawnInterval);
            timerPanel.transform.DOLocalMoveY(112, 0.3f).SetEase(Ease.OutBack);
        }));
        // 提升难度：提升敌人属性
        foreach (var enemyData in GameManager.Instance.RuntimeEnemyConfigDict.Values)
        {
            enemyData.damage = Mathf.Min(100, enemyData.damage++);
            enemyData.health = Mathf.Min(9999, enemyData.health + 10);
            enemyData.moveSpeed = Mathf.Min(50, enemyData.moveSpeed++);
        }
        // 提升难度：增加高权重敌人的出现概率
        UpgradeHighWeightEnemy();
        // 重置状态
        isWaitingForClear = false;
        currentWaveSpawnedCount = 0; // 重置已生成数量
        timer = 0;
    }

    /// <summary>
    /// 提升高权重敌人的权重（难度递增）
    /// </summary>
    private void UpgradeHighWeightEnemy()
    {
        foreach (var type in enemyTypes.Where(type => highWeightEnemyNames.Contains(type.prefabName)))
        {
            type.weight += waveAddHighWeight;
            break;
        }
    }
    
    #endregion

    #region 生成敌人
    
    // 控制协程，避免同一时间多协程执行
    private Coroutine spawnCoroutine;
    private void SpawnEnemyBatch()
    {
        // 计算本次能生成的数量 = 单次批量数 和 剩余可生成数 的最小值
        int remainToSpawn = currentMaxEnemies - currentWaveSpawnedCount;
        int canSpawnCount = Mathf.Min(currentBatchSize, remainToSpawn);
        if (canSpawnCount <= 0) return;
        // 如果已有生成协程在运行，先停止（避免重复生成）
        if (spawnCoroutine != null) { StopCoroutine(spawnCoroutine); }
        // 启动协程，间隔生成敌人
        spawnCoroutine = StartCoroutine(SpawnEnemyBatchCoroutine(canSpawnCount));
    }
    
    /// <summary>
    /// 协程：间隔生成敌人（每个敌人间隔0.2秒）
    /// </summary>
    /// <param name="spawnCount">本次要生成的数量</param>
    private IEnumerator SpawnEnemyBatchCoroutine(int spawnCount)
    {
        // 维护本次批量生成的位置列表，用于检测同批次间距
        List<Vector3> currentBatchSpawnPos = new List<Vector3>();
        for (int i = 0; i < spawnCount; i++)
        {
            // 1. 按权重随机选择敌人类型
            string enemyToSpawn = GetRandomEnemyByWeight();
            if (string.IsNullOrEmpty(enemyToSpawn))
            {
                Debug.LogWarning("未找到有效敌人类型！");
                // 即使类型无效，也保留0.2秒间隔，保持节奏
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            // 2. 生成随机位置（仅与本次批量生成的位置保持≥2单位间距）
            Vector3 spawnPos = GetRandomSpawnPosForBatch(currentBatchSpawnPos);
            // 3. 将本次生成的位置加入列表（供后续敌人检测间距）
            currentBatchSpawnPos.Add(spawnPos);
            // 4. 从对象池获取并生成敌人
            GameObject enemy = PoolMgr.Instance.GetObj(enemyToSpawn);
            if (enemy != null)
            {
                enemy.name = enemyToSpawn;
                enemy.transform.position = spawnPos;
                enemy.transform.rotation = Quaternion.identity;
                // 每生成一个，更新当前波次已生成数量
                currentWaveSpawnedCount++;
            }
            else { Debug.LogWarning($"对象池未找到：{enemyToSpawn}，请检查预加载和预制体路径"); }
            // 5. 每个敌人生成后，间隔 0.2秒再生成下一个
            if (i < spawnCount - 1) // 最后一个敌人无需等待
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        // 协程结束后重置引用
        spawnCoroutine = null;
    }
    
    /// <summary>
    /// 为单次批量生成获取随机位置（仅与本次批量的位置保持最小间距）
    /// </summary>
    /// <param name="batchPosList">本次批量已生成的位置列表</param>
    /// <returns>符合间距要求的位置</returns>
    private Vector3 GetRandomSpawnPosForBatch(List<Vector3> batchPosList)
    {
        Vector3 validPos = Vector3.zero;
        int retryCount = 0;
        // 最大重试次数（避免死循环，可根据需要调整）
        int maxRetry = 50;

        while (retryCount < maxRetry)
        {
            // 生成基础随机位置（远离玩家的逻辑不变）
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = 0; // 固定Y轴，保持水平平面
            randomDir.Normalize();
            float randomDistance = Random.Range(spawnRadius * 0.1f, spawnRadius);
            validPos = Vector3.zero + randomDir * randomDistance;

            // 检测与本次批量已生成位置的间距
            bool isPosValid = true;
            foreach (Vector3 pos in batchPosList)
            {
                // 检测距离是否小于最小间距（默认2单位）
                if (Vector3.Distance(validPos, pos) < minEnemyDistance)
                {
                    isPosValid = false;
                    break;
                }
            }
            // 位置有效则退出循环
            if (isPosValid) { break; }
            retryCount++;
        }
        // 重试耗尽时的兜底提示（一般不会触发，除非批量生成数量过多）
        if (retryCount >= maxRetry)
        {
            Debug.LogWarning($"同批次生成位置重试{maxRetry}次，使用最后生成的位置（可能间距不足）");
        }
        return validPos;
    }

    /// <summary>
    /// 按权重随机选择敌人类型（增加空值校验）
    /// </summary>
    private string GetRandomEnemyByWeight()
    {
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogError("敌人类型列表为空！");
            return string.Empty;
        }
        // 计算总权重
        int totalWeight = 0;
        foreach (var type in enemyTypes)
        {
            totalWeight += type.weight;
        }
        // 总权重为0时返回第一个
        if (totalWeight <= 0)
        {
            Debug.LogWarning("敌人总权重为0，返回第一个类型");
            return enemyTypes[0].prefabName;
        }
        // 随机取值并匹配敌人类型
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (var type in enemyTypes)
        {
            currentWeight += type.weight;
            if (randomValue < currentWeight)
            {
                return type.prefabName;
            }
        }
        // 默认返回第一个
        return enemyTypes[0].prefabName;
    }
    
    #endregion

    #region 下雨效果

    private bool isRain;
    public Light enviormentLight;
    public Light playerLight;
    private void RandomStartRain()
    {
        // 生成0-1的随机数，判断是否触发下雨
        float randomValue = Random.Range(0f, 1f);
        if (randomValue <= 0.5f)
        {
            if (isRain) { return; }
            // 触发下雨
            for (int i = 0; i < rains.Count; i++) { rains[i].gameObject.SetActive(true); }
            enviormentLight.DOIntensity(0, 5f);
            playerLight.DOIntensity(10, 5f);
            isRain = true;
        }
        else
        {
            for (int i = 0; i < rains.Count; i++) { rains[i].gameObject.SetActive(false); }
            isRain = false;
            enviormentLight.DOIntensity(1, 5f);
            playerLight.DOIntensity(0, 5f);
        }
    }

    #endregion
    
    #region 预加载对象池
    private void PreloadAllPools()
    {
        PoolMgr.Instance.Clear();
        PreloadEnemy("Cow", CowPoolName, 30);
        PreloadEnemy("redWarnTip", RedWarnPoolName, 30);
        PreloadEnemy("YellowFireExplo", ExploPoolName, 15);
        PreloadEnemy("StarFish", StarFishPoolName, 30);
        PreloadEnemy("BlueSlime", BlueSlimePoolName, 30);
        PreloadEnemy("multiBullet", MultiBulletPoolName, 100);
        PreloadEnemy("MultiRobot", MultiRobotPoolName, 30);
        PreloadEnemy("BloodHitRed", HitPlayerBloodPoolName, 20);
        PreloadEnemy("SingleBullet", SingleBulletPoolName, 100);
        PreloadEnemy("TurtleShell", TurtleShellPoolName, 30);
        PreloadEnemy("SingleRobot", SingleRobotPoolName, 30);
        PreloadEnemy("ParabolicBullet", ParabolicBulletPoolName, 30);
    }

    /// <summary>
    /// 通用预加载方法
    /// </summary>
    /// <param name="resPath">资源路径</param>
    /// <param name="poolName">池子名称</param>
    /// <param name="count">预加载数量</param>
    private void PreloadEnemy(string resPath, string poolName, int count)
    {
        for (var i = 0; i < count; i++)
        {
            ResMgr.Instance.LoadAndInstantiateAsync(resPath,null,(obj =>
            {
                if (obj != null)
                {
                    obj.name = poolName;
                    PoolMgr.Instance.PushObj(poolName, obj);
                }
                else { Debug.LogWarning($"预加载失败：{resPath} 不存在！"); }
            }));
            
        }
    }
    
    #endregion

    #region 外部调用
    
    /// <summary>
    /// 获取当前波次
    /// </summary>
    public int GetCurrentWave() { return currentWave; }

    /// <summary>
    /// 重置无尽模式
    /// </summary>
    public void ResetEndlessMode()
    {
        currentWave = 1;
        currentBatchSize = initBatchSize;
        currentMaxEnemies = initMaxEnemies;
        currentSpawnInterval = initSpawnInterval;
        isWaitingForClear = false;
        timer = 0;
        // 重置敌人权重
        foreach (var type in enemyTypes) { type.weight = type.initWeight; }
        Debug.Log($"【无尽模式】已重置！第{currentWave}波开始");
    }
    
    #endregion

}