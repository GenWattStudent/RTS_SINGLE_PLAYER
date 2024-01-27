using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SlotData {
    public TemplateContainer templateContainer;
    public BuildingSo buildingSo;
}

public class UIBuildingManager : MonoBehaviour
{
    [SerializeField] private BuildingSo[] buildings;
    private UIDocument UIDocument;

    private BuildingSo selectedBuilding;

    private VisualElement root;
    private VisualElement slotContainer;
    public VisualTreeAsset visualTree;
    private List<SlotData> slots = new ();
    public static UIBuildingManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        slotContainer = root.Q<VisualElement>("TabContent");
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

        slots.Clear();
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

    private void SetBuildingData(TemplateContainer buildingTab, BuildingSo buildingSo) {
        var buildingNameText = buildingTab.Q<Label>("SlotName");
        var costText = buildingTab.Q<Label>("SlotValue");
        var valueIcon = buildingTab.Q<VisualElement>("ValueIcon");
        var image = buildingTab.Q<VisualElement>("ImageBox");
        var slot = buildingTab.Q<VisualElement>("Slot");
        var quantityText = buildingTab.Q<Label>("Quantity");
        var progressBar = buildingTab.Q<ProgressBar>("ProgressBarTimer");

        quantityText.style.display = DisplayStyle.None;
        progressBar.style.display = DisplayStyle.None;

        buildingNameText.text = buildingSo.buildingName;
        costText.text = buildingSo.cost.ToString();
        valueIcon.style.backgroundImage = new StyleBackground(buildingSo.costResource.icon);

        if (image is not null) {
            image.style.backgroundImage = new StyleBackground(buildingSo.sprite);
        }

        UpdateSlot(buildingSo, buildingTab);

        slot.RegisterCallback((ClickEvent ev) => {
            OnSlotClick(buildingSo);
        });
    }

    public void CreateBuildingTab(BuildingSo building) {
        TemplateContainer buildingTab = visualTree.Instantiate();
        buildingTab.name = building.buildingName;
        buildingTab.style.height = Length.Percent(100);

        SetBuildingData(buildingTab, building);
        slotContainer.Add(buildingTab);
        slots.Add(new SlotData { buildingSo = building, templateContainer = buildingTab });
    }

    private void UpdateSlot(BuildingSo buildingSo, TemplateContainer container) {
        if (!UIStorage.Instance.HasEnoughResource(buildingSo.costResource, buildingSo.cost)) {
            container.SetEnabled(false);
        } else {
            container.SetEnabled(true);
        }
    }

    private void FixedUpdate() {
        foreach (var slotData in slots) {
            UpdateSlot(slotData.buildingSo, slotData.templateContainer);
        }   
    }
}
