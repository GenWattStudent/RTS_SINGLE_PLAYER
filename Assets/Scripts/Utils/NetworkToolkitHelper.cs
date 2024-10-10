
using Unity.Netcode;
using UnityEngine.UIElements;

public class NetworkToolkitHelper : NetworkBehaviour
{
    public UIDocument UIDocument;
    public VisualElement root;

    protected virtual void OnEnable()
    {
        if (UIDocument == null) UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
    }

    public Label GetLabel(string name)
    {
        return root.Q<Label>(name);
    }

    public Button GetButton(string name)
    {
        return root.Q<Button>(name);
    }

    public VisualElement GetVisualElement(string name)
    {
        return root.Q<VisualElement>(name);
    }

    public ProgressBar GetProgressBar(string name)
    {
        return root.Q<ProgressBar>(name);
    }

    public void AddClasses(VisualElement element, string[] classes)
    {
        foreach (var className in classes)
        {
            element.AddToClassList(className);
        }
    }

    public void RemoveClasses(VisualElement element, string[] classes)
    {
        foreach (var className in classes)
        {
            element.RemoveFromClassList(className);
        }
    }

    public void SetSuccess(VisualElement element)
    {
        RemoveClasses(element, new string[] { "text-danger", "text-warning" });
        AddClasses(element, new string[] { "text-success" });
    }

    public void SetWarning(VisualElement element)
    {
        RemoveClasses(element, new string[] { "text-danger", "text-success" });
        AddClasses(element, new string[] { "text-warning" });
    }

    public void SetDanger(VisualElement element)
    {
        RemoveClasses(element, new string[] { "text-success", "text-warning" });
        AddClasses(element, new string[] { "text-danger" });
    }
}
