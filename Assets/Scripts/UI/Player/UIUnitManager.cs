using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIUnitManager : NetworkToolkitHelper
{
    public GameObject currentBuilding;
    public VisualTreeAsset slot;

    private Spawner _spawnerBuilding;
    private VisualElement _unitSlotContainer;
    private List<SpawnerSlot> _spawnerSlots = new();
    private SelectionManager _selectionManager;
    private BuildingLevelable _buildingLevelable;
    private UIStorage _uiStorage;

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
        _selectionManager = GetComponentInParent<SelectionManager>();
        _uiStorage = GetComponentInParent<UIStorage>();
        _unitSlotContainer = root.Q<VisualElement>("TabContent");

        SelectionManager.OnSelect += HandleSelection;
    }

    private void OnDisable()
    {
        SelectionManager.OnSelect -= HandleSelection;
    }

    private void HandleSelection(List<Selectable> selectedObjects)
    {
        if (_spawnerBuilding is not null)
        {
            _spawnerBuilding.OnCurrentUnitChange -= HandleAddUnitToQueue;
            _spawnerBuilding.OnAddUnitToQueue -= HandleAddUnitToQueue;
            if (_buildingLevelable != null) _buildingLevelable.level.OnValueChanged -= HandleLevelChange;
            _spawnerBuilding = null;
        }

        CreateUnitSlots(selectedObjects);
    }

    private void CreateUnitSlots(List<Selectable> selectedObjects)
    {
        if (selectedObjects.Count == 1 && selectedObjects[0].selectableType == Selectable.SelectableType.Building)
        {
            var building = selectedObjects[0].gameObject.GetComponent<Building>();
            var construction = building.GetComponent<Construction>();
            _spawnerBuilding = building.GetComponent<Spawner>();
            _buildingLevelable = building.GetComponent<BuildingLevelable>();

            if (building.buildingSo.unitsToSpawn.Count() > 0 && construction == null)
            {
                CreateUnitTabs(building.buildingSo, _spawnerBuilding);
            }
        }
    }

    private void HandleAddUnitToQueue(UnitSo unitSo)
    {
        HandleSelection(_selectionManager.selectedObjects);
    }

    private void Clear()
    {
        foreach (var spawnerSlot in _spawnerSlots)
        {
            spawnerSlot.Clear();
        }

        _spawnerSlots.Clear();
        _unitSlotContainer.Clear();
    }

    private void HandleLevelChange(int oldValue, int newValue)
    {
        HandleSelection(_selectionManager.selectedObjects);
    }

    public void CreateUnitTabs(BuildingSo BuildingSo, Spawner spawnerBuilding)
    {
        Clear();
        spawnerBuilding.OnCurrentUnitChange += HandleAddUnitToQueue;
        spawnerBuilding.OnAddUnitToQueue += HandleAddUnitToQueue;

        if (_buildingLevelable != null) _buildingLevelable.level.OnValueChanged += HandleLevelChange;

        var currentSpawningUnit = spawnerBuilding.GetCurrentSpawningUnit();

        foreach (var soUnit in BuildingSo.unitsToSpawn)
        {
            var unitQueueCount = spawnerBuilding.GetUnitQueueCountByName(soUnit.unitName);
            var spawnerSlot = new SpawnerSlot(slot, soUnit, spawnerBuilding.GetComponent<Building>(), unitQueueCount, _uiStorage);

            if (currentSpawningUnit is not null && currentSpawningUnit.unitName == soUnit.unitName)
            {
                spawnerSlot.SetSpawnData(spawnerBuilding.spawnTimer);
            }

            _spawnerSlots.Add(spawnerSlot);
            _unitSlotContainer.Add(spawnerSlot.Slot);
        }
    }
}
