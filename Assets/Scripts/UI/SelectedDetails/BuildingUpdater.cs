using UnityEngine;
using UnityEngine.UIElements;

public class BuildingDetailsUpdater
{
    private VisualElement statsContainer;
    private ProgressBar healthBar;
    private ProgressBar expirenceBar;
    private Label levelText;
    private VisualElement actions;
    private UIStorage uIStorage;
    private Button levelUpButton;
    private Button sellButton;
    private VisualElement attackActions;

    public BuildingDetailsUpdater(
        VisualElement statsContainer,
        ProgressBar healthBar,
        ProgressBar expirenceBar,
        Label levelText,
        VisualElement actions,
        UIStorage uIStorage,
        Button levelUpButton,
        Button sellButton,
        VisualElement attackActions)
    {
        this.statsContainer = statsContainer;
        this.healthBar = healthBar;
        this.expirenceBar = expirenceBar;
        this.levelText = levelText;
        this.actions = actions;
        this.uIStorage = uIStorage;
        this.levelUpButton = levelUpButton;
        this.sellButton = sellButton;
        this.attackActions = attackActions;
    }

    public void UpdateBuildingDetails(Selectable selectable)
    {
        actions.style.display = DisplayStyle.Flex;
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();
        var construction = selectable.GetComponent<Construction>();

        if (damagable != null && building != null)
        {
            float health = damagable.stats.GetStat(StatType.Health);
            float maxHealth = damagable.stats.GetStat(StatType.MaxHealth);


            if (building.buildingSo.incomeResource != null)
            {
                var income = damagable.stats.GetStat(StatType.Income);
                var incomeResource = building.buildingSo.incomeResource.resourceName;

                StatCreator.CreateIncomeStat(statsContainer, income);
                StatCreator.CreateIncomeResourceStat(statsContainer, incomeResource);
            }

            UpdateHealthBar(health, maxHealth);
            HandleLeveling(building, damagable);
            ActivateBuildingCamera(damagable);

            if (damagable.damagableSo.canAttack)
            {
                StatCreator.CreateDamageStat(statsContainer, damagable.stats.GetStat(StatType.Damage), damagable.stats.GetBaseStat(StatType.Damage));
                ShowHideAttackActions(true);
            }
            else
            {
                ShowHideAttackActions(false);
            }

            if (
                construction != null ||
                (building.buildingLevelable != null && building.buildingLevelable.maxLevel <= building.buildingLevelable.level.Value))
            {
                // construction (hide level up button)
                levelUpButton.style.display = DisplayStyle.None;
                sellButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                // building (show level up button)
                levelUpButton.style.display = DisplayStyle.Flex;
                sellButton.style.display = DisplayStyle.Flex;
            }
        }
    }

    private void ShowHideAttackActions(bool show)
    {
        attackActions.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void HandleLeveling(Building building, Damagable damagable)
    {
        // Handle level and experience display logic here
        if (building.buildingLevelable != null)
        {
            if (building.buildingLevelable.maxLevel > building.buildingLevelable.level.Value)
            {
                // show level and activate buttons
                levelText.text = $"{building.buildingLevelable.level.Value} LVL";
            }
            else
            {
                // max level
                levelText.text = $"MAX {building.buildingLevelable.level.Value} LVL";
            }

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

            UpdateExpirenceBar();
        }
    }

    private void UpdateHealthBar(float health, float maxHealth)
    {
        healthBar.lowValue = 0;
        healthBar.value = health;
        healthBar.highValue = maxHealth;
        healthBar.title = $"HP: {health}/{maxHealth}";
    }

    private void UpdateExpirenceBar()
    {
        expirenceBar.style.display = DisplayStyle.None;
    }

    private void ActivateBuildingCamera(Damagable damagable)
    {
        var camera = damagable.GetComponentInChildren<Camera>(true);
        camera?.gameObject.SetActive(true);
    }
}
