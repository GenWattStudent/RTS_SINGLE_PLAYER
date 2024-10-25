using System;
using UnityEngine.UIElements;

public class UnitSlot
{
    public VisualElement Slot;
    public UnitSo SoUnit;
    public int UnitCount;

    public event Action<UnitSo> OnClick;

    public UnitSlot(VisualTreeAsset slot, UnitSo soUnit, int unitCount)
    {
        Slot = slot.Instantiate();
        Slot.name = soUnit.unitName;

        SoUnit = soUnit;
        UnitCount = unitCount;

        SetUnitData();
    }

    private void HandleClick(ClickEvent evt)
    {
        OnClick?.Invoke(SoUnit);
    }

    private void SetUnitData()
    {
        var unitCount = Slot.Q<Label>("UnitCount");
        var unitImage = Slot.Q<VisualElement>("UnitImage");

        Slot.RegisterCallback<ClickEvent>(HandleClick);

        unitCount.text = UnitCount.ToString();

        if (SoUnit.sprite != null)
        {
            unitImage.style.backgroundImage = new StyleBackground(SoUnit.sprite);
        }
    }
}