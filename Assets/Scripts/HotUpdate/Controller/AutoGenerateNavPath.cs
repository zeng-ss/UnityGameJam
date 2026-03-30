using UnityEngine;
using UnityEngine.AI; // 必须引入NavMesh命名空间

[RequireComponent(typeof(LineRenderer))]
public class AutoGenerateNavPath : MonoBehaviour
{
    [Header("路径配置")]
    public float lineWidth = 0.2f; // 导航线宽度
    public Transform startTarget;  // 寻路起点
    private LineRenderer lineRenderer;
    public Transform endTarget;    // 寻路终点
    [HideInInspector] public bool isStart;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.positionCount = 0; // 初始隐藏
    }

    void Update()
    {
        // 每帧自动更新路径（起点/终点移动时，路径自动刷新）
        if (isStart && startTarget != null && endTarget != null) { GenerateAutoNavPath(startTarget.position, endTarget.position); }
    }

    /// <summary>
    /// 核心：自动生成寻路路径（无需手动赋值路径点）
    /// </summary>
    public void GenerateAutoNavPath(Vector3 startPos, Vector3 endPos)
    {
        // 1. 初始化NavMesh路径
        NavMeshPath navPath = new NavMeshPath();
        // 2. 自动计算起点→终点的路径（基于场景NavMesh）
        bool isPathValid = NavMesh.CalculatePath(
            startPos, 
            endPos, 
            NavMesh.AllAreas, // 匹配所有NavMesh区域
            navPath
        );

        // 3. 路径有效则赋值给LineRenderer
        if (isPathValid && navPath.corners.Length >= 2)
        {
            lineRenderer.positionCount = navPath.corners.Length;
            // 把自动生成的路径点赋值给导航线
            for (int i = 0; i < navPath.corners.Length; i++)
            {
                //Vector3 pathPoint = navPath.corners[i];
                
                // 新增：将世界坐标转换为路径对象自身的本地坐标 这是解决偏移问题的核心
                Vector3 localPoint = transform.InverseTransformPoint(navPath.corners[i]);
                lineRenderer.SetPosition(i, localPoint);
            }
        }
        else { lineRenderer.positionCount = 0; } // 路径无效则隐藏
    }
}