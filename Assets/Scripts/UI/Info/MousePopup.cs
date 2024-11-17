using UnityEngine;
using UnityEngine.UIElements;

public class MousePopup : NetworkToolkitHelper
{
    public bool IsPopupOpen = false;
    private VisualElement container;
    private Label popupText;
    private float margin = 10f;

    public static MousePopup Instance { get; private set; }

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        Instance = this;
        container = GetVisualElement("MousePopup");
        popupText = GetLabel("MousePopupMessage");
    }

    public void SetText(string text)
    {
        popupText.text = text;
    }

    public void Show()
    {
        IsPopupOpen = true;
        container.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        IsPopupOpen = false;
        container.style.display = DisplayStyle.None;
    }

    public void SetPosition(Vector3 position)
    {
        container.transform.position = position;
    }

    public void SetPosition(Vector3 position, Vector3 offset)
    {
        container.style.top = Screen.height - position.y - offset.y;
        container.style.left = position.x - offset.x;
    }

    public void SetMargin(float margin)
    {
        this.margin = margin;
    }

    private void Update()
    {
        if (IsPopupOpen)
        {
            // convert mouse position to screen position to set here the popup with offset
            var mousePosition = Input.mousePosition;
            var offset = new Vector3(margin, -margin, 0);

            SetPosition(mousePosition, offset);
        }
    }
}

