using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UIUnitManager : NetworkBehaviour
{
    private List<VisualElement> unitSlotTabs = new();
    private List<UnitSo> unitsAttachedToTab = new();
    private BuildingSo selectedBuilding;
    private Spawner spawnerBuilding;
    public GameObject currentBuilding;
    public bool IsUnitUIOpen { get; set; } = false;
    public bool IsUnitSelectionTabOpen { get; set; } = false;
    private Dictionary<string, List<UnitSo>> unitQueue = new();
    private int unitCountPrev = 0;
    private UIDocument UIDocument;
    private VisualElement root;
    public VisualTreeAsset slot;
    private VisualElement unitSlotContainer;
    private SelectionManager selectionManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        unitSlotContainer = root.Q<VisualElement>("TabContent");

        SelectionManager.OnSelect += CreateSelectionUnitTab;
    }

    private void OnDisable()
    {
        SelectionManager.OnSelect -= CreateSelectionUnitTab;
    }

    private void UpdateProgressBar(ProgressBar progressBar, float currentTime, float totalSpawnTime)
    {
        progressBar.lowValue = 0;
        progressBar.highValue = totalSpawnTime;
        progressBar.value = currentTime;
    }

    public void SetSpawnData(VisualElement slot, int unitQueueCount, float currentTime, float totalSpawnTime)
    {
        var spawnUnitCountText = slot.Q<Label>("Quantity");
        var progressTime = slot.Q<ProgressBar>("ProgressBarTimer");

        spawnUnitCountText.style.display = DisplayStyle.Flex;
        progressTime.style.display = DisplayStyle.Flex;

        var timeRounded = Mathf.RoundToInt(currentTime);

        progressTime.title = timeRounded.ToString() + "s";
        spawnUnitCountText.text = unitQueueCount.ToString() + "x";
        // Debug.Log("SetSpawnData: " + currentTime + "Total: " + totalSpawnTime);
        UpdateProgressBar(progressTime, currentTime, totalSpawnTime);
    }

    private void HideSpawnInfo(VisualElement unitTab)
    {
        var spawnUnitCountText = unitTab.Q<Label>("Quantity");
        var progressTime = unitTab.Q<ProgressBar>("ProgressBarTimer");

        spawnUnitCountText.style.display = DisplayStyle.None;
        progressTime.style.display = DisplayStyle.None;
    }

    private void FixedUpdate()
    {
        if (!IsUnitSelectionTabOpen && IsUnitUIOpen && spawnerBuilding is not null)
        {
            foreach (var unitTab in unitSlotTabs)
            {
                var currentTime = spawnerBuilding.GetSpawnTimer();
                var currentSpawningUnit = spawnerBuilding.GetCurrentSpawningUnit();
                var unitQueueCount = spawnerBuilding.GetUnitQueueCountByName(unitTab.name);
                var soUnit = unitsAttachedToTab.Find(x => x.unitName == unitTab.name);

                if (currentSpawningUnit is not null && currentSpawningUnit.unitName == unitTab.name)
                {
                    // if current spawning unit is the same as the unit tab, update the progress bar
                    SetSpawnData(unitTab, unitQueueCount, currentTime, spawnerBuilding.totalSpawnTime.Value);
                }

                if (currentSpawningUnit is not null && currentSpawningUnit.unitName != unitTab.name && unitQueueCount > 0)
                {
                    SetSpawnData(unitTab, unitQueueCount, 0, spawnerBuilding.totalSpawnTime.Value);
                }

                if (unitQueueCount <= 0)
                {
                    HideSpawnInfo(unitTab);
                }

                var buildingLevelable = currentBuilding.GetComponent<BuildingLevelable>();

                ToogleTabEnableBasedOnLevel(buildingLevelable, soUnit, unitTab);
            }
        }
    }

    private void ToogleTabEnableBasedOnLevel(BuildingLevelable buildingLevelable, UnitSo unitSo, VisualElement unitTab)
    {
        var quantityText = unitTab.Q<Label>("Quantity");
        if (buildingLevelable != null && buildingLevelable.level.Value < unitSo.spawnerLevelToUnlock)
        {
            unitTab.SetEnabled(false);

            quantityText.style.display = DisplayStyle.Flex;
            quantityText.text = $"Level {unitSo.spawnerLevelToUnlock} required";
        }
        else
        {
            unitTab.SetEnabled(true);
        }
    }

    public void CreateSelectionUnitTab()
    {
        if (unitCountPrev == 0 && selectionManager.selectedObjects.Count == 0) return;
        if (selectionManager.selectedObjects.Count == 1 && selectionManager.IsBuilding(selectionManager.selectedObjects[0])) return;

        unitCountPrev = selectionManager.selectedObjects.Count;
        ClearTabs();
        unitSlotTabs.Clear();
        unitsAttachedToTab.Clear();
        unitQueue.Clear();

        Debug.Log("CreateSelectionUnitTab");
        foreach (var selectable in selectionManager.selectedObjects)
        {
            if (selectable.selectableType == Selectable.SelectableType.Unit)
            {
                var unit = selectable.GetComponent<Unit>();
                var unitSo = unit.unitSo;

                if (unitQueue.ContainsKey(unitSo.unitName))
                {
                    unitQueue[unitSo.unitName].Add(unitSo);
                }
                else
                {
                    unitQueue.Add(unitSo.unitName, new List<UnitSo> { unitSo });
                }
            }
        }

        foreach (var unit in unitQueue)
        {
            TemplateContainer templateContainer = slot.Instantiate();
            templateContainer.name = unit.Key;
            templateContainer.style.height = Length.Percent(100);
            var unitQueueCount = unit.Value.Count;

            SetUnitData(templateContainer, unit.Value[0], unitQueueCount);

            var progressBar = templateContainer.Q<ProgressBar>("ProgressBarTimer");
            var quantityText = templateContainer.Q<Label>("Quantity");

            quantityText.style.display = DisplayStyle.Flex;
            progressBar.style.display = DisplayStyle.None;

            unitSlotTabs.Add(templateContainer);
            unitsAttachedToTab.Add(unit.Value[0]);
            unitSlotContainer.Add(templateContainer);
        }

        IsUnitUIOpen = true;
        IsUnitSelectionTabOpen = true;
    }

    private void SetUnitData(VisualElement unitTab, UnitSo soUnit, int cost = -1)
    {
        var unitNameText = unitTab.Q<Label>("SlotName");
        var button = unitTab.Q<VisualElement>("Slot");

        unitNameText.text = soUnit.unitName;

        if (cost < 0 && soUnit.cost > 0)
        {
            var costText = unitTab.Q<Label>("SlotValue");
            var valueIcon = unitTab.Q<VisualElement>("ValueIcon");
            valueIcon.style.backgroundImage = new StyleBackground(soUnit.costResource.icon);
            costText.text = soUnit.cost.ToString();

            button.RegisterCallback((ClickEvent ev) =>
            {
                var unitsToSpawn = currentBuilding.GetComponent<Building>().buildingSo.unitsToSpawn;
                for (int i = 0; i < unitsToSpawn.Count(); i++)
                {
                    if (unitsToSpawn[i].unitName == soUnit.unitName)
                    {
                        spawnerBuilding.AddUnitToQueueServerRpc(i);
                    }
                }
            });
        }
        else
        {
            var costText = unitTab.Q<Label>("Quantity");
            var value = unitTab.Q<VisualElement>("SlotValueBox");

            value.style.display = DisplayStyle.None;
            costText.text = cost.ToString();
        }

        VisualElement image = unitTab.Q<VisualElement>("ImageBox");

        if (image is not null)
        {
            image.style.backgroundImage = new StyleBackground(soUnit.sprite);
        }
    }

    public void ClearTabs()
    {
        List<VisualElement> visualElements = new();

        foreach (var child in unitSlotContainer.Children())
        {
            visualElements.Add(child);
        }

        foreach (var visualElement in visualElements)
        {
            unitSlotContainer.Remove(visualElement);
        }
    }

    public void CreateUnitTabs(BuildingSo BuildingSo, Spawner spawnerBuilding, GameObject building)
    {
        ClearTabs();
        unitSlotTabs.Clear();
        unitsAttachedToTab.Clear();
        selectedBuilding = BuildingSo;
        this.spawnerBuilding = spawnerBuilding;
        currentBuilding = building;

        var currentTime = spawnerBuilding.GetSpawnTimer();
        var currentSpawningUnit = spawnerBuilding.GetCurrentSpawningUnit();

        foreach (var soUnit in BuildingSo.unitsToSpawn)
        {
            TemplateContainer unitTab = slot.Instantiate();
            unitTab.name = soUnit.unitName;
            unitTab.style.height = Length.Percent(100);
            var unitQueueCount = spawnerBuilding.GetUnitQueueCountByName(soUnit.unitName);
            var buildingLevelable = building.GetComponent<BuildingLevelable>();

            if (currentSpawningUnit is not null && currentSpawningUnit.unitName == soUnit.unitName)
            {
                SetSpawnData(unitTab, unitQueueCount, currentTime, spawnerBuilding.totalSpawnTime.Value);
            }
            else if (unitQueueCount > 0)
            {
                SetSpawnData(unitTab, unitQueueCount, currentTime, spawnerBuilding.totalSpawnTime.Value);
            }

            // HideSpawnInfo(unitTab);
            ToogleTabEnableBasedOnLevel(buildingLevelable, soUnit, unitTab);
            SetUnitData(unitTab, soUnit);

            unitSlotTabs.Add(unitTab);
            unitsAttachedToTab.Add(soUnit);
            unitSlotContainer.Add(unitTab);
        }

        IsUnitUIOpen = true;
        IsUnitSelectionTabOpen = false;
    }
}
