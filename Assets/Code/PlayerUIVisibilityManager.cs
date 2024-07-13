using UnityEngine;
using UnityEngine.UI;

public class PlayerUIVisibilityManager : MonoBehaviour
{
    [SerializeField] private GameObject starterAssetsInputsCanvas;
    [SerializeField] private Button enterAndExitButton;
    [SerializeField] private MSSceneControllerFree mSSceneControllerFree;
    [SerializeField] private DebugCarAnimationManager debugCarAnimationManager;
    private bool isShowPlayerUI;

    private void Start()
    {
        enterAndExitButton.onClick.AddListener(() => UIVisibility());

        if (mSSceneControllerFree.startInPlayer)
        {
            isShowPlayerUI = true;
        }
        else
        {
            isShowPlayerUI = false;
        }

        UIVisibility();
    }

    public void UIVisibility()
    {
        if (isShowPlayerUI)
        {
            starterAssetsInputsCanvas.SetActive(true);
        }
        else
        {
            starterAssetsInputsCanvas.SetActive(false);
        }

        isShowPlayerUI = !isShowPlayerUI;
    }
}