using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnerSlot
{
    public VisualElement Slot;
    public UnitSo SoUnit;
    public ISpawnerBuilding Spawner;
    public Building Building;
    public int Quantity;
    public NetworkVariable<float> Timer;

    private VisualElement _imageBox;
    private VisualElement _valueIcon;
    private Label _slotName;
    private Label _slotValue;
    private Label _quantity;
    private ProgressBar _progressBarTimer;
    private UIStorage _uiStorage;

    public SpawnerSlot(VisualTreeAsset slot, UnitSo unitSo, Building building, int quantity, UIStorage uiStorage)
    {
        Slot = slot.Instantiate();
        Slot.style.height = Length.Percent(100);
        Slot.name = unitSo.unitName;

        SoUnit = unitSo;
        Building = building;
        Spawner = building.GetComponent<ISpawnerBuilding>();
        Quantity = quantity;

        _uiStorage = uiStorage;
        _imageBox = Slot.Q<VisualElement>("ImageBox");
        _valueIcon = Slot.Q<VisualElement>("ValueIcon");
        _slotName = Slot.Q<Label>("SlotName");
        _slotValue = Slot.Q<Label>("SlotValue");
        _quantity = Slot.Q<Label>("Quantity");
        _progressBarTimer = Slot.Q<ProgressBar>("ProgressBarTimer");

        _progressBarTimer.style.display = DisplayStyle.None;

        SetSpawnerData();
    }

    private void HandleTimerChange(float oldValue, float newValue)
    {
        UpdateTimer(newValue);
    }

    public void SetSpawnData(NetworkVariable<float> timer)
    {
        Timer = timer;
        Timer.OnValueChanged += HandleTimerChange;

        _progressBarTimer.style.display = DisplayStyle.Flex;
        _progressBarTimer.lowValue = 0;
        _progressBarTimer.highValue = SoUnit.spawnTime;

        UpdateTimer(timer.Value);
    }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
        _quantity.text = GetQuantityText();
    }

    public void UpdateTimer(float timer)
    {
        _progressBarTimer.value = timer;
        _progressBarTimer.title = $"{Mathf.RoundToInt(timer)}s";
    }

    public void Clear()
    {
        if (Timer == null) return;

        Timer.OnValueChanged -= HandleTimerChange;
    }

    private void HandleClick(ClickEvent evt)
    {
        var index = Building.buildingSo.unitsToSpawn.IndexOf(SoUnit);
        Spawner.AddUnitToQueue(index);
    }

    private string GetQuantityText()
    {
        var buildingLevelable = Building.GetComponent<BuildingLevelable>();

        if (buildingLevelable != null && buildingLevelable.level.Value < SoUnit.spawnerLevelToUnlock)
        {
            Slot.SetEnabled(false);
            return $"Level {SoUnit.spawnerLevelToUnlock} required";
        }

        if (!_uiStorage.HasEnoughResource(SoUnit.costResource, SoUnit.cost))
        {
            Slot.SetEnabled(false);
            return "Not enough resources";
        }

        Slot.SetEnabled(true);
        return Quantity == 0 ? "" : $"{Quantity}x";
    }

    public void SetSpawnerData()
    {
        _slotName.text = SoUnit.unitName;

        _valueIcon.style.backgroundImage = new StyleBackground(SoUnit.costResource.icon);
        _slotValue.text = SoUnit.cost.ToString();
        SetQuantity(Quantity);

        Slot.RegisterCallback<ClickEvent>(HandleClick);

        if (SoUnit.sprite != null)
        {
            _imageBox.style.backgroundImage = new StyleBackground(SoUnit.sprite);
        }
    }
}
