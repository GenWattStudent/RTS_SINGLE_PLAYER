using UnityEngine;
using UnityEngine.UIElements;

public class UnitDetailsUpdater
{
    private VisualElement statsContainer;
    private ProgressBar healthBar;
    private ProgressBar expirenceBar;
    private Label levelText;
    private VisualElement actions;
    private Button levelUpButton;
    private Button sellButton;
    private VisualElement attackActions;

    public UnitDetailsUpdater(
        VisualElement statsContainer,
        ProgressBar healthBar,
        ProgressBar expirenceBar,
        Label levelText,
        VisualElement actions,
        Button levelUpButton,
        Button sellButton,
        VisualElement attackActions)
    {
        this.statsContainer = statsContainer;
        this.healthBar = healthBar;
        this.expirenceBar = expirenceBar;
        this.levelText = levelText;
        this.actions = actions;
        this.levelUpButton = levelUpButton;
        this.sellButton = sellButton;
        this.attackActions = attackActions;
    }

    public void UpdateUnitDetails(Stats stats)
    {
        actions.style.display = DisplayStyle.Flex;
        expirenceBar.style.display = DisplayStyle.Flex;
        levelUpButton.style.display = DisplayStyle.None;
        sellButton.style.display = DisplayStyle.None;

        var health = stats.GetStat(StatType.Health);
        var maxHealth = stats.GetStat(StatType.MaxHealth);
        var damage = stats.GetStat(StatType.Damage);
        var damagable = stats.GetComponent<Damagable>();

        StatCreator.CreateHealthStat(statsContainer, health, maxHealth);
        StatCreator.CreateDamageStat(statsContainer, damage);

        if (damagable.damagableSo.canAttack)
        {
            ShowHideAttackActions(true);
        }
        else
        {
            ShowHideAttackActions(false);
        }

        UpdateExpirenceStat(damagable);
        UpdateHealthBar(health, maxHealth);
        ActivateUnitCamera(damagable);
    }

    private void ShowHideAttackActions(bool show)
    {
        attackActions.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateExpirenceStat(Damagable damagable)
    {
        expirenceBar.lowValue = 0;
        expirenceBar.value = damagable.levelable.expirence.Value;
        expirenceBar.highValue = damagable.levelable.expirenceToNextLevel.Value;
        expirenceBar.title = $"EXP: {damagable.levelable.expirence.Value}/{damagable.levelable.expirenceToNextLevel.Value}";
        levelText.text = $"{damagable.levelable.level.Value} LVL";
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
        var camera = damagable.GetComponentInChildren<Camera>(true);
        camera?.gameObject.SetActive(true);
    }
}
