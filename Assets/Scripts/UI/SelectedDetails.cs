using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Selectable;

public class SelectedDetails : ToolkitHelper
{
    private Building building;
    private Button levelUpButton;
    private Button sellButton;
    private VisualElement statsContainer;
    private Label levelText;
    private ProgressBar healthBar;
    private ProgressBar expirenceBar;
    private VisualElement selectionInfo;
    private VisualElement actions;
    private bool isGoToTab = true;

    private void Start()
    {
        selectionInfo = root.Q<VisualElement>("SelectionInfo");
        levelUpButton = root.Q<Button>("LevelUp");
        sellButton = root.Q<Button>("Sell");
        statsContainer = root.Q<VisualElement>("Stats");
        levelText = root.Q<Label>("Level");
        healthBar = root.Q<ProgressBar>("Healthbar");
        expirenceBar = root.Q<ProgressBar>("Expirencebar");
        actions = GetVisualElement("Actions");

        levelUpButton.RegisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClick);

        ActivateButtons(false);
    }

    private void OnDisable() {
        // levelUpButton.UnregisterCallback<ClickEvent>(OnUpgradeButtonClick);
        // sellButton.UnregisterCallback<ClickEvent>(OnSellButtonClick);
    }

    private void ActivateButtons(bool isActive) {
        levelUpButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        sellButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnUpgradeButtonClick(ClickEvent ev) {
        building.buildingLevelable.LevelUp();
    }

    private void OnSellButtonClick(ClickEvent ev) {
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
        CreateStat("Health", $"{damagable.health}/{damagable.maxHealth}");
    }

    private void CreateDamageStat(Damagable damagable) {
        CreateStat("Damage", $"{damagable.damage}");
    }

    private void CreateExpirenceStat(Damagable damagable) {
        UpdateExpirenceBar(damagable);
        levelText.text = $"{damagable.levelable.level} LVL";
    }

    private void ShowHideAttackActions(bool isActive) {
        var attackActions = GetVisualElement("AttackActions");
        attackActions.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
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
        healthBar.highValue = damagable.maxHealth;
        healthBar.title = $"HP: {damagable.health}/{damagable.maxHealth}";
    }

    private void ActivateUnitCamera(Damagable damagable) {
        if (damagable == null) return;
        var camera = damagable.GetComponentInChildren<Camera>(true);
        if (camera == null) return;
        camera.gameObject.SetActive(true);
    }

    private void UpdateUnitDetails(Damagable damagable)
    {
        actions.style.display = DisplayStyle.Flex;
        expirenceBar.style.display = DisplayStyle.Flex;
        CreateHealthStat(damagable);
        CreateDamageStat(damagable);
        CreateExpirenceStat(damagable);
        ActivateUnitCamera(damagable);
        UpdateHealthBar(damagable);
        ActivateButtons(false);

        if (damagable.damagableSo.canAttack) {
            ShowHideAttackActions(true);
        } else {
            ShowHideAttackActions(false);
        }
    }

    private void UpdateBuildingDetails(Selectable selectable)
    {
        actions.style.display = DisplayStyle.Flex;
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();

        CreateHealthStat(damagable);

        if (building.buildingLevelable != null && building.buildingSo.unitsToSpawn.GetLength(0) > 0) {
            CreateStat("Spawn time reduction", $"{building.buildingLevelable.reduceSpawnTime}");
        }

        if (building != null) {
            ActivateUnitCamera(damagable);

            if (building.attackableSo != null) {
                CreateDamageStat(damagable);
                ShowHideAttackActions(true);
            } else {
                ShowHideAttackActions(false);
            }

            UpdateHealthBar(damagable);
            expirenceBar.style.display = DisplayStyle.None;

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
                levelText.text = $"MAX {building.buildingLevelable.level} LVL";
                levelUpButton.style.display = DisplayStyle.None;
                sellButton.style.display = DisplayStyle.Flex;
            }
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
        Show();
        actions.style.display = DisplayStyle.None;
        CreateStat("Selected", $"{SelectionManager.selectedObjects.Count} units");
        ActivateButtons(false);
    }

    private void UpdateSelectedDetails()
    {
        ClearStats();

        if (SelectionManager.selectedObjects.Count == 0) {
            Hide();
            if (!isGoToTab) {
                var tabs = System.Enum.GetValues(typeof(BuildingSo.BuildingType));
                var tabName = tabs.GetValue(0).ToString();
                UITabManagement.Instance.HandleTabClick(UITabManagement.Instance.GetTab(tabName));
                isGoToTab = true;
            }

            return;
        };

        isGoToTab = false;

        if (SelectionManager.selectedObjects.Count == 1)
        {
            Show();
            var selectable = SelectionManager.selectedObjects[0];
            var unit = selectable.GetComponent<Unit>();
            var damagable = selectable.GetComponent<Damagable>();

            if (unit != null && selectable.selectableType == SelectableType.Unit)
            {
                UpdateUnitDetails(damagable);
            }
            else
            {
                UpdateBuildingDetails(selectable);
            }
        }
        else
        {
            UpdateMultipleDetails();
        }
    }

    private void FixedUpdate()
    {
        UpdateSelectedDetails();
    }
}
