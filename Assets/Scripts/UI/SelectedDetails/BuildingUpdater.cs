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

    public BuildingDetailsUpdater(VisualElement statsContainer, ProgressBar healthBar, ProgressBar expirenceBar, Label levelText, VisualElement actions, UIStorage uIStorage)
    {
        this.statsContainer = statsContainer;
        this.healthBar = healthBar;
        this.expirenceBar = expirenceBar;
        this.levelText = levelText;
        this.actions = actions;
        this.uIStorage = uIStorage;
    }

    public void UpdateBuildingDetails(Selectable selectable)
    {
        actions.style.display = DisplayStyle.Flex;
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();

        if (damagable != null && building != null)
        {
            float health = damagable.stats.GetStat(StatType.Health);
            float maxHealth = damagable.stats.GetStat(StatType.MaxHealth);

            StatCreator.CreateHealthStat(statsContainer, health, maxHealth);
            UpdateHealthBar(health, maxHealth);
            HandleLeveling(building, damagable);
            ActivateBuildingCamera(damagable);
        }
    }

    private void HandleLeveling(Building building, Damagable damagable)
    {
        // Handle level and experience display logic here
        if (building.buildingLevelable != null)
        {
            var nextLevel = building.buildingLevelable.GetNextBuildingLevel();
            if (nextLevel != null && uIStorage.HasEnoughResource(nextLevel.resourceSO, nextLevel.cost))
            {
                levelText.text = $"{building.buildingLevelable.level.Value} LVL";
            }
            else
            {
                levelText.text = $"MAX {building.buildingLevelable.level.Value} LVL";
            }
        }
    }

    private void UpdateHealthBar(float health, float maxHealth)
    {
        healthBar.lowValue = 0;
        healthBar.value = health;
        healthBar.highValue = maxHealth;
        healthBar.title = $"HP: {health}/{maxHealth}";
    }

    private void ActivateBuildingCamera(Damagable damagable)
    {
        var camera = damagable.GetComponentInChildren<Camera>(true);
        camera?.gameObject.SetActive(true);
    }
}
