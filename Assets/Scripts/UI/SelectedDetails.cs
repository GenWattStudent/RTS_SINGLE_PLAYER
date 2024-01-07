using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Selectable;

public class SelectedDetails : MonoBehaviour
{
    UIDocument UIDocument;
    private Building building;
    private VisualElement root;
    private Button levelUpButton;
    private Button sellButton;
    private VisualElement statsContainer;
    private VisualElement image;
    private Label levelText;
    private ProgressBar healthBar;
    private ProgressBar expirenceBar;
    private VisualElement selectionInfo;
    private Damagable damagable;

    private void Start()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        selectionInfo = root.Q<VisualElement>("SelectionInfo");
        levelUpButton = root.Q<Button>("LevelUp");
        sellButton = root.Q<Button>("Sell");
        statsContainer = root.Q<VisualElement>("Stats");
        image = root.Q<VisualElement>("Image");
        levelText = root.Q<Label>("Level");
        healthBar = root.Q<ProgressBar>("Healthbar");
        expirenceBar = root.Q<ProgressBar>("Expirencebar");

        levelUpButton.RegisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClick);

        ActivateButtons(false);
    }

    private void OnDisable() {
        levelUpButton.UnregisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.UnregisterCallback<ClickEvent>(OnSellButtonClick);
    }

    private void ActivateButtons(bool isActive) {
        levelUpButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        sellButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnUpgradeButtonClick(ClickEvent ev) {
        Debug.Log("Upgrade button click");
        building.buildingLevelable.LevelUp();
    }

    private void OnSellButtonClick(ClickEvent ev) {
        Debug.Log("Sell button click");
        building.Sell();
    }

    private void CreateStat(string name, string value) {
        var statBox = new VisualElement
        {
            name = name
        };

        statBox.AddToClassList("stat");
        var statLabel = new Label(name);
        var statValue = new Label(value);

        statBox.Add(statLabel);
        statBox.Add(statValue);

        statsContainer.Add(statBox);
    }

    private void CreateHealthStat(Damagable damagable) {
        CreateStat("Health", $"{damagable.health}/{damagable.damagableSo.health}");
    }

    private void CreateDamageStat(AttackableSo attackableSo) {
        CreateStat("Damage", $"{attackableSo.bulletSo.damage}");
    }

    private void CreateExpirenceStat(Damagable damagable) {
        UpdateExpirenceBar(damagable);
        levelText.text = $"{damagable.levelable.level} LVL";
    }

    private void ClearStats() {
        List<VisualElement> elementsToRemove = new ();

        foreach (var stat in statsContainer.Children())
        {
            elementsToRemove.Add(stat);
        }

        foreach (var stat in elementsToRemove)
        {
            statsContainer.Remove(stat);
        }

        ActivateButtons(false);
    }

    private void UpdateExpirenceBar(Damagable damagable) {
        expirenceBar.lowValue = 0;
        expirenceBar.value = damagable.levelable.expirence;
        expirenceBar.highValue = damagable.levelable.expirenceToNextLevel;
        expirenceBar.title = $"EXP: {damagable.levelable.expirence}/{damagable.levelable.expirenceToNextLevel}";
    }

    private void UpdateHealthBar(Damagable damagable) {
        healthBar.lowValue = 0;
        healthBar.value = damagable.health;
        healthBar.highValue = damagable.damagableSo.health;
        healthBar.title = $"HP: {damagable.health}/{damagable.damagableSo.health}";
    }

    private void ActivateUnitCamera(Damagable damagable) {
        if (damagable == null) return;
        var camera = damagable.GetComponentInChildren<Camera>(true);
        Debug.Log("Camera: " + camera);
        if (camera == null) return;
        Debug.Log("Activate unit camera");  
        camera.gameObject.SetActive(true);
    }
    private void UpdateUnitDetails(Unit unit, Damagable damagable)
    {
        CreateHealthStat(damagable);
        CreateDamageStat(unit.attackableSo);
        CreateExpirenceStat(damagable);
        ActivateUnitCamera(damagable);

        UpdateHealthBar(damagable);
        this.damagable = damagable;
    }

    private void UpdateBuildingDetails(Selectable selectable)
    {
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();

        CreateHealthStat(damagable);

        if (building.buildingLevelable != null && building.buildingSo.unitsToSpawn.GetLength(0) > 0) {
            CreateStat("Spawn time reduction", $"{building.buildingLevelable.reduceSpawnTime}");
        }

        if (building != null) {
            ActivateUnitCamera(damagable);

            if (building.attackableSo != null) {
                CreateDamageStat(building.attackableSo);
            }

            UpdateHealthBar(damagable);

            var construction = selectable.GetComponent<Construction>();

            if (construction != null) {
                this.building = building;
                levelUpButton.style.display = DisplayStyle.None;
                sellButton.style.display = DisplayStyle.Flex;
                return;
            }

            if (building.buildingLevelable != null && building.buildingLevelable.maxLevel > building.buildingLevelable.level) {
                levelText.text = $"{building.buildingLevelable.level} LVL";
                this.building = building;
                ActivateButtons(true);
            } else {
                levelUpButton.style.display = DisplayStyle.None;
                sellButton.style.display = DisplayStyle.Flex;
            }

            this.damagable = damagable;
        }
    }

    private void Hide()
    {
        selectionInfo.style.display = DisplayStyle.None;
    }

    private void Show()
    {
        selectionInfo.style.display = DisplayStyle.Flex;
    }

    private void UpdateMultipleDetails()
    {
        Hide();
        // Debug.Log("Update multiple details");
    }

    private void UpdateSelectedDetails()
    {
        ClearStats();
        Debug.Log("Update selected details " + SelectionManager.selectedObjects.Count);
        if (SelectionManager.selectedObjects.Count == 0) {
            Hide();
            return;
        };

        if (SelectionManager.selectedObjects.Count == 1)
        {
            Show();
            var selectable = SelectionManager.selectedObjects[0];
            var unit = selectable.GetComponent<Unit>();
            var damagable = selectable.GetComponent<Damagable>();

            if (unit != null && selectable.selectableType == SelectableType.Unit)
            {
                UpdateUnitDetails(unit, damagable);
            }
            else
            {
                UpdateBuildingDetails(selectable);
            }
        }
        else
        {
            this.damagable = null;
            UpdateMultipleDetails();
        }
    }

    private void FixedUpdate()
    {
        UpdateSelectedDetails();
    }
}
