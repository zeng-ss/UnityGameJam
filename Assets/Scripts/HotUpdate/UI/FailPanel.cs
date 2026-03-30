using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class FailPanel : BasePanel
{
    public Button backButton;

    private void Start()
    {
        backButton.onClick.AddListener(BackClick);
    }

    private void BackClick()
    {
        SceneMgr.Instance.LoadSceneAsync("StartScene", () =>
        {
            MaskSystemManager.Instance.IsMaskSelected = false;
            UIManager.Instance.closePanel<FailPanel>();
            UIManager.Instance.closePanel<PlayerProPanel>();
        });
    }
}
