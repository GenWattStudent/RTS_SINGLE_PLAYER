using UnityEngine.UIElements;

public static class StatCreator
{
    public static void CreateHealthStat(VisualElement statsContainer, float health, float maxHealth)
    {
        CreateStat(statsContainer, "Health", $"{health}/{maxHealth}");
    }

    public static void CreateDamageStat(VisualElement statsContainer, float damage)
    {
        CreateStat(statsContainer, "Damage", $"{damage}");
    }

    public static void CreateBuildingSpeedStat(VisualElement statsContainer, float speed)
    {
        CreateStat(statsContainer, "Building Speed", $"{speed}");
    }

    public static void CreateAttackSpeedStat(VisualElement statsContainer, float attackSpeed)
    {
        CreateStat(statsContainer, "Attack Speed", $"{attackSpeed}");
    }

    public static void CreateBuildingDistanceStat(VisualElement statsContainer, float distance)
    {
        CreateStat(statsContainer, "Building Distance", $"{distance}");
    }

    public static void CreateStat(VisualElement statsContainer, string name, string value)
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
}
