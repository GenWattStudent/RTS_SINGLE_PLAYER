using UnityEngine.UIElements;

public class SceneLoader : ToolkitHelper
{
    private VisualElement sceneLoading;
    private ProgressBar loadingProgressbar;

    protected override void OnEnable()
    {
        base.OnEnable();

        sceneLoading = GetVisualElement("SceneLoading");
        loadingProgressbar = root.Q<ProgressBar>("LoadingProgressbar");
    }

    private void OnDisable()
    {
        sceneLoading.style.display = DisplayStyle.None;
    }

    public void ShowSceneLoading()
    {
        sceneLoading.style.display = DisplayStyle.Flex;
    }

    public void HideSceneLoading()
    {
        sceneLoading.style.display = DisplayStyle.None;
    }

    public void SetProgress(int progress)
    {
        loadingProgressbar.value = progress * 100;
        loadingProgressbar.title = $"Loading({progress * 100}%)";
    }
}
