using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InfoBox : ToolkitHelper
{
    private VisualElement infoBox;
    [SerializeField] private int maxMessages = 4; 
    [SerializeField] private float timeToHide = 5f;
    private List<Label> messages = new ();
    public bool IsOpen = false;
    private float currentTime = 0f;

    public static InfoBox Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        infoBox = GetVisualElement("InfoBox");
        ClearAllMessages();
        Hide();
    }

    private void ClearAllMessages() {
        List<VisualElement> labels = new ();

        foreach (var label in infoBox.Children())
        {
            labels.Add(label);
        }

        foreach (var label in labels)
        {
            infoBox.Remove(label);
        }

        messages.Clear();
    }

    private void RemoveMessage() {
        if (messages.Count > 0) {
            infoBox.Remove(messages[0]);
            messages.RemoveAt(0);
        }
    }

    public void Show() {
        IsOpen = true;
        currentTime = 0f;
        infoBox.style.display = DisplayStyle.Flex;
    }

    public void Hide() {
        IsOpen = false;
        infoBox.style.display = DisplayStyle.None;
    }

    public void AddError(string error) {
        var errorLabel = new Label(error);
        AddClasses(errorLabel, new string[] { "text-danger", "text-medium" } );
        errorLabel.style.whiteSpace = WhiteSpace.Normal;

        if (messages.Count >= maxMessages) {
            RemoveMessage();
        }

        infoBox.Add(errorLabel);
        messages.Add(errorLabel);
        Show();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpen) {
            currentTime += Time.deltaTime;

            if (currentTime >= timeToHide) {
                currentTime = 0f;
                Hide();
            }
        }
    }
}
