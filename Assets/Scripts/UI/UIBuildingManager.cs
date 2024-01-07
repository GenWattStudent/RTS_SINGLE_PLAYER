using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIBuildingManager : Singleton<UIBuildingManager>
{
    [SerializeField] private BuildingSo[] buildings;
    private UIDocument UIDocument;

    // [SerializeField] private Button upgradeButton;
    private BuildingSo selectedBuilding;

    private VisualElement root;
    private VisualElement slotContainer;
    public VisualTreeAsset visualTree;

    private void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        slotContainer = root.Q<VisualElement>("TabContent");
        Debug.Log("Slot container: " + slotContainer);
    }

    public void ClearTabs() {
        List<VisualElement> tabs = new ();

        foreach (var tab in slotContainer.Children())
        {
            tabs.Add(tab);
        }

        foreach (var tab in tabs)
        {
            slotContainer.Remove(tab);
        }
    }

    public BuildingSo GetSelectedBuilding() {
        return selectedBuilding;
    }

    public void SetSelectedBuilding(BuildingSo building) {
        selectedBuilding = building;
    }

    public void CreateBuildingTabs(BuildingSo.BuildingType buildingType) {
        ClearTabs();
        UIUnitManager.Instance.IsUnitUIOpen = false;
        UIUnitManager.Instance.IsUnitSelectionTabOpen = false;
        // upgradeButton.gameObject.SetActive(false);

        foreach (var building in buildings)
        {
            if (building.type == buildingType)
            {
                CreateBuildingTab(building);
            }
        }
    }

    private void OnSlotClick(BuildingSo buildingSo) {
        Debug.Log("Clicked on " + buildingSo.buildingName);
        if (!UIStorage.Instance.HasEnoughResource(buildingSo.costResource, buildingSo.cost)) return;
        selectedBuilding = buildingSo;
    }

    private void SetBuildingData(TemplateContainer buildingTab, BuildingSo BuildingSo) {
        var buildingNameText = buildingTab.Q<Label>("SlotName");
        var costText = buildingTab.Q<Label>("SlotValue");
        var image = buildingTab.Q<VisualElement>("ImageBox");
        var slot = buildingTab.Q<VisualElement>("Slot");
        var quantityText = buildingTab.Q<Label>("Quantity");
        var progressBar = buildingTab.Q<ProgressBar>("ProgressBarTimer");

        quantityText.style.display = DisplayStyle.None;
        progressBar.style.display = DisplayStyle.None;

        buildingNameText.text = BuildingSo.buildingName;
        costText.text = BuildingSo.cost.ToString();

        slot.RegisterCallback((ClickEvent ev) => {
            OnSlotClick(BuildingSo);
        });

        if (image is not null) {
            Debug.Log("Image is not null");
            image.style.backgroundImage = new StyleBackground(BuildingSo.sprite);
        }
    }

    public void CreateBuildingTab(BuildingSo bulding) {
        TemplateContainer buildingTab = visualTree.Instantiate();
        buildingTab.name = bulding.buildingName;

        SetBuildingData(buildingTab, bulding);
        slotContainer.Add(buildingTab);
    }
}
