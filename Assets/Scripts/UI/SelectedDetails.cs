using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Selectable;

public class SelectedDetails : NetworkToolkitHelper
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
    private SelectionManager selectionManager;
    private UIStorage uIStorage;
    private UITabManagement uITabManagement;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        selectionInfo = root.Q<VisualElement>("SelectionInfo");
        levelUpButton = root.Q<Button>("LevelUp");
        sellButton = root.Q<Button>("Sell");
        statsContainer = root.Q<VisualElement>("Stats");
        levelText = root.Q<Label>("Level");
        healthBar = root.Q<ProgressBar>("Healthbar");
        expirenceBar = root.Q<ProgressBar>("Expirencebar");
        actions = GetVisualElement("Actions");

        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
        var playerController = selectionManager.GetComponent<PlayerController>();
        uIStorage = playerController.GetComponentInChildren<UIStorage>();
        uITabManagement = playerController.GetComponentInChildren<UITabManagement>();

        levelUpButton.RegisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClick);

        ActivateButtons(false);
    }

    private void OnDisable()
    {

        // levelUpButton.UnregisterCallback<ClickEvent>(OnUpgradeButtonClick);
        // sellButton.UnregisterCallback<ClickEvent>(OnSellButtonClick);
    }

    private void ActivateButtons(bool isActive)
    {
        levelUpButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        sellButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnUpgradeButtonClick(ClickEvent ev)
    {
        if (building.buildingLevelable != null)
        {
            building.buildingLevelable.LevelUpServerRpc();
        }
    }

    private void OnSellButtonClick(ClickEvent ev)
    {
        Debug.Log("SellButtonClick " + building.buildingSo.buildingName + " " + building.buildingSo.cost);
        building.SellServerRpc();
    }

    private void CreateStat(string name, string value)
    {
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

    private void CreateHealthStat(float health, float maxHealth)
    {
        CreateStat("Health", $"{health}/{maxHealth}");
    }

    private void CreateDamageStat(float damage)
    {
        CreateStat("Damage", $"{damage}");
    }

    private void CreateExpirenceStat(Damagable damagable)
    {
        UpdateExpirenceBar(damagable);
        levelText.text = $"{damagable.levelable.level.Value} LVL";
    }

    private void ShowHideAttackActions(bool isActive)
    {
        var attackActions = GetVisualElement("AttackActions");
        attackActions.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ClearStats()
    {
        List<VisualElement> elementsToRemove = new();

        foreach (var stat in statsContainer.Children())
        {
            elementsToRemove.Add(stat);
        }

        foreach (var stat in elementsToRemove)
        {
            statsContainer.Remove(stat);
        }
    }

    private void UpdateExpirenceBar(Damagable damagable)
    {
        expirenceBar.lowValue = 0;
        expirenceBar.value = damagable.levelable.expirence.Value;
        expirenceBar.highValue = damagable.levelable.expirenceToNextLevel.Value;
        expirenceBar.title = $"EXP: {damagable.levelable.expirence.Value}/{damagable.levelable.expirenceToNextLevel.Value}";
    }

    private void UpdateHealthBar(float health, float maxHealth)
    {
        healthBar.lowValue = 0;
        healthBar.value = health;
        healthBar.highValue = maxHealth;
        healthBar.title = $"HP: {health}/{maxHealth}";
    }

    private void ActivateUnitCamera(Damagable damagable)
    {
        if (damagable == null) return;
        var camera = damagable.GetComponentInChildren<Camera>(true);
        if (camera == null) return;
        camera.gameObject.SetActive(true);
    }

    private void UpdateUnitDetails(Stats stats)
    {
        actions.style.display = DisplayStyle.Flex;
        expirenceBar.style.display = DisplayStyle.Flex;

        var health = stats.GetStat(StatType.Health);
        var maxHealth = stats.GetStat(StatType.MaxHealth);
        var damage = stats.GetStat(StatType.Damage);
        var damagable = stats.GetComponent<Damagable>();

        CreateHealthStat(health, maxHealth);
        CreateDamageStat(damage);
        CreateExpirenceStat(damagable);
        ActivateUnitCamera(damagable);
        UpdateHealthBar(health, maxHealth);
        ActivateButtons(false);

        if (damagable.damagableSo.canAttack)
        {
            ShowHideAttackActions(true);
        }
        else
        {
            ShowHideAttackActions(false);
        }
    }

    private void UpdateBuildingDetails(Selectable selectable)
    {
        actions.style.display = DisplayStyle.Flex;
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();
        var health = damagable.stats.GetStat(StatType.Health);
        var maxHealth = damagable.stats.GetStat(StatType.MaxHealth);
        this.building = building;

        CreateHealthStat(health, maxHealth);

        if (building.buildingLevelable != null && building.buildingSo.unitsToSpawn.GetLength(0) > 0)
        {
            // spawn time reduction 
            CreateStat("Spawn time reduction", $"{building.buildingLevelable.reduceSpawnTime.Value}");
        }

        if (building != null)
        {
            ActivateUnitCamera(damagable);

            if (building.attackableSo != null)
            {
                // damage
                var damage = damagable.stats.GetStat(StatType.Damage);
                CreateDamageStat(damage);
                ShowHideAttackActions(true);
            }
            else
            {
                // no damage
                ShowHideAttackActions(false);
            }
            // health
            UpdateHealthBar(health, maxHealth);
            expirenceBar.style.display = DisplayStyle.None;

            var construction = selectable.GetComponent<Construction>();

            if (construction != null)
            {
                // construction (hide level up button)
                levelUpButton.style.display = DisplayStyle.None;
                sellButton.style.display = DisplayStyle.Flex;
                return;
            }

            if (building.buildingLevelable != null && building.buildingLevelable.maxLevel > building.buildingLevelable.level.Value)
            {
                // show level and activate buttons
                levelText.text = $"{building.buildingLevelable.level.Value} LVL";
                ActivateButtons(true);
            }
            else
            {
                // max level
                levelText.text = $"MAX {building.buildingLevelable.level.Value} LVL";
                levelUpButton.style.display = DisplayStyle.None;
                sellButton.style.display = DisplayStyle.Flex;
            }

            if (building.buildingLevelable != null)
            {
                // check if enugh resources to level up
                var nextBuildingLevel = building.buildingLevelable.GetNextBuildingLevel();

                if (nextBuildingLevel != null && uIStorage.HasEnoughResource(nextBuildingLevel.resourceSO, nextBuildingLevel.cost))
                {
                    levelUpButton.SetEnabled(true);
                }
                else
                {
                    levelUpButton.SetEnabled(false);
                }
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
        CreateStat("Selected", $"{selectionManager.selectedObjects.Count} units");
        ActivateButtons(false);
    }

    private void UpdateSelectedDetails()
    {
        ClearStats();

        if (selectionManager.selectedObjects.Count == 0)
        {
            Hide();
            if (!isGoToTab)
            {
                var tabs = System.Enum.GetValues(typeof(BuildingSo.BuildingType));
                var tabName = tabs.GetValue(0).ToString();
                uITabManagement.HandleTabClick(uITabManagement.GetTab(tabName));
                isGoToTab = true;
            }

            return;
        };

        isGoToTab = false;

        if (selectionManager.selectedObjects.Count == 1)
        {
            Show();
            var selectable = selectionManager.selectedObjects[0];
            var unit = selectable.GetComponent<Unit>();
            var stats = selectable.GetComponent<Stats>();

            if (unit != null && selectable.selectableType == SelectableType.Unit)
            {
                UpdateUnitDetails(stats);
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
