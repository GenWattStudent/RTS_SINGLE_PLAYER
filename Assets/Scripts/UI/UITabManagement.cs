using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UITabManagement : MonoBehaviour
{
    private List<Button> tabs = new ();
    public string CurrentTab { get; private set; }
    private UIDocument UIDocument;
    private VisualElement root;
    private VisualElement tabContainer;
    public static UITabManagement Instance { get; private set; }

    public void OnTabClick(string tabName)
    {
        CurrentTab = tabName;
        UIBuildingManager.Instance.CreateBuildingTabs((BuildingSo.BuildingType)System.Enum.Parse(typeof(BuildingSo.BuildingType), tabName));
    }

    private void CreateTabs()
    {
        // Make tabs from enum BuildingSo
        var unitTypes = System.Enum.GetValues(typeof(BuildingSo.BuildingType));

        foreach (var unitType in unitTypes)
        {
            var tabName = unitType.ToString();
            Button tab = new Button
            {
                name = tabName,
                text = tabName
            };

            tab.AddToClassList("btn-primary");
            tab.AddToClassList("btn-rounded-small");
            tab.AddToClassList("btn-medium");

            tab.RegisterCallback<ClickEvent>(ev => {
                tabs.ForEach(tab => tab.RemoveFromClassList("active"));
                tab.AddToClassList("active");
                OnTabClick(tabName);
            });

            // Add tab to list
            tabs.Add(tab);
            tabContainer.Add(tab);
        }
    }

    private void ClearButtons()
    {
        List<Button> buttons = new ();

        foreach (var tab in tabContainer.Children())
        {
            buttons.Add((Button)tab);
        }

        foreach (var tab in buttons)
        {
            tabContainer.Remove(tab);
        }
    }

    void Start()
    {
        Instance = this;
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        tabContainer = root.Q<VisualElement>("BuildingTabs");

        ClearButtons();
        CreateTabs();
        var tabs = System.Enum.GetValues(typeof(BuildingSo.BuildingType));
        var economyTab = tabs.GetValue(0).ToString();
        OnTabClick(economyTab);
    }
}
