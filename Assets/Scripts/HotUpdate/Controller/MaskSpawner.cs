using UnityEngine;

public class MaskSpawner : MonoBehaviour
{
    private void Start()
    {
        SpawnMasks();
    }
    
    /// <summary>
    /// 生成三个独立旋转的面具
    /// </summary>
    private void SpawnMasks()
    {
        float angleStep = 360f / 5; // 每个护盾间隔 72度
        for (int i = 0; i < 5; i++)
        {
            // 加载预制体
            ResMgr.Instance.LoadAndInstantiateAsync("Mask",null,(mask =>
            {
                if (mask == null)
                {
                    Debug.LogError("护盾预制体加载失败！");
                }
                mask.name = "Mask_" + i;
                // 角度转弧度
                float currentAngle = angleStep * i * Mathf.Deg2Rad;
                float x = Mathf.Cos(currentAngle) * 40;
                float z = Mathf.Sin(currentAngle) * 40;
                // 跟随角色位置，保持高度1f
                mask.transform.position = new Vector3(x, 1f, z);
            }));
            
        }
    }
    
}
