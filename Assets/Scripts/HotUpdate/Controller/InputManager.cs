using DG.Tweening;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundAudioPool.Instance.PlaySound(SoundAudioPool.Instance.showPanelClip, transform.position);
            UIManager.Instance.openPanel<EscPanel>((escPanel =>
            {
                escPanel.transform.DOLocalMove(Vector3.zero, 0.5f);
            }));
        }
    }
}
