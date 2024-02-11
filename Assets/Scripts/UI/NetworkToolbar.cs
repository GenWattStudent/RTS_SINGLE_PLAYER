using UnityEngine.UIElements;

public class NetworkToolbar : NetworkToolkitHelper
{
    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            var gameUi = GetVisualElement("GameUI");
            gameUi.style.display = DisplayStyle.None;
        }
    }
}
