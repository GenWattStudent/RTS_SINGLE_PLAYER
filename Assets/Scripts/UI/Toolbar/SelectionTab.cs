using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectionTab : NetworkToolkitHelper
{
    [SerializeField] private VisualTreeAsset slot;

    private VisualElement unitSlotContainer;
    private SelectionManager selectionManager;
    private Dictionary<string, List<UnitSo>> selectedUnitGropus = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        unitSlotContainer = GetVisualElement("UnitSlotContainer");
    }

    private void Start()
    {
        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
        SelectionManager.OnSelect += HandleSelection;
    }

    private void HandleSelection(List<Selectable> selectedObjects)
    {
        CreateSelectionUnitTab(selectedObjects);
    }

    private void GroupUnitsBasedOnType(List<Selectable> selectedObjects)
    {
        foreach (var selectable in selectedObjects)
        {
            if (selectable.selectableType == Selectable.SelectableType.Unit)
            {
                var unit = selectable.GetComponent<Unit>();
                var unitSo = unit.unitSo;

                if (selectedUnitGropus.ContainsKey(unitSo.unitName))
                {
                    selectedUnitGropus[unitSo.unitName].Add(unitSo);
                }
                else
                {
                    selectedUnitGropus.Add(unitSo.unitName, new List<UnitSo> { unitSo });
                }
            }
        }
    }

    private void Reset()
    {
        unitSlotContainer.Clear();
        unitSlotContainer.style.display = DisplayStyle.None;
        selectedUnitGropus.Clear();
    }

    private void HandleSlotClick(UnitSo unitSo)
    {
        var itemsToDeselect = new List<Selectable>();

        foreach (var selectable in selectionManager.selectedObjects)
        {
            var unit = selectable.GetComponent<Unit>();

            if (unit.unitSo.unitName != unitSo.unitName)
            {
                itemsToDeselect.Add(selectable);
            }
        }

        foreach (var item in itemsToDeselect)
        {
            selectionManager.Deselect(item);
        }
    }

    private void CreateSelectionUnitTab(List<Selectable> selectedObjects)
    {
        if (selectedObjects.Count <= 1)
        {
            Reset();
            return;
        }

        Reset();

        Debug.Log("CreateSelectionUnitTab");
        GroupUnitsBasedOnType(selectedObjects);

        foreach (var unit in selectedUnitGropus)
        {
            var unitSlot = new UnitSlot(slot, unit.Value[0], unit.Value.Count);

            unitSlot.OnClick += HandleSlotClick;

            unitSlotContainer.Add(unitSlot.Slot);
        }

        unitSlotContainer.style.display = DisplayStyle.Flex;
    }
}
