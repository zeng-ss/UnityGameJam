using UnityEngine;
using System.Collections;

public class ExploController : MonoBehaviour
{
    private string ExploPoolName = "Explo";
    
    private void OnEnable()
    {
        StartCoroutine(RecycleAfterDelay(2));
    }

    // 延迟回收协程
    private IEnumerator RecycleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolMgr.Instance.PushObj(ExploPoolName, gameObject);
    }
    
}