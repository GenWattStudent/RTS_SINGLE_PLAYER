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

    private void Start()
    {
        Debug.Log("Selected details start");
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        Debug.Log("Selected details started " + root.name);
        levelUpButton = root.Q<Button>("LevelUp");
        sellButton = root.Q<Button>("Sell");
        statsContainer = root.Q<VisualElement>("Stats");
        image = root.Q<VisualElement>("Image");
        levelText = root.Q<Label>("Level");
        healthBar = root.Q<ProgressBar>("Healthbar");
        expirenceBar = root.Q<ProgressBar>("Expirencebar");
        Debug.Log("Selected details started " + levelUpButton.text);

        ActivateButtons(false);
    }

    private void OnEnable() {
        SelectionManager.OnSelect += UpdateSelectedDetails;
        levelUpButton.RegisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClick);
    }

    private void OnDisable() {
        SelectionManager.OnSelect -= UpdateSelectedDetails;
        levelUpButton.UnregisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.UnregisterCallback<ClickEvent>(OnSellButtonClick);
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
        Debug.Log("Created stat " + name);
        Debug.Log(" LOL" + statsContainer.name);
        statsContainer.Add(statBox);
    }

    private void CreateHealthStat(Damagable damagable) {
        CreateStat("Health", $"{damagable.health}/{damagable.damagableSo.health}");
    }

    private void CreateDamageStat(AttackableSo attackableSo) {
        CreateStat("Damage", $"{attackableSo.bulletSo.damage}");
    }

    private void CreateExpirenceStat(Damagable damagable) {
        CreateStat("Expirence", $"{damagable.levelable.expirence}");
        expirenceBar.lowValue = 0;
        expirenceBar.highValue = damagable.levelable.expirenceToNextLevel;
        levelText.text = $"{damagable.levelable.level} LVL";
    }

    private void ClearStats() {
        statsContainer.Clear();
        // healthBar.UpdateProgresBar(0, 0);
        image.style.backgroundImage = null;
        ActivateButtons(false);
    }

    private void UpdateHealthBar(Damagable damagable) {
        healthBar.lowValue = 0;
        Debug.Log(damagable.health);
        healthBar.value = damagable.health;
        healthBar.highValue = damagable.damagableSo.health;
    }

    private void UpdateUnitDetails(Unit unit, Damagable damagable)
    {
        CreateHealthStat(damagable);
        CreateDamageStat(unit.attackableSo);
        CreateExpirenceStat(damagable);
        image.style.backgroundImage = new StyleBackground(unit.unitSo.sprite);

        UpdateHealthBar(damagable);
    }

    private void UpdateBuildingDetails(Selectable selectable)
    {
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();
        CreateHealthStat(damagable);

        if (building != null) {
           image.style.backgroundImage = new StyleBackground(building.buildingSo.sprite);

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
        }
    }

    private void UpdateMultipleDetails()
    {
        // Debug.Log("Update multiple details");
    }

    private void UpdateSelectedDetails()
    {
        ClearStats();
        if (SelectionManager.selectedObjects.Count == 0) return;

        if (SelectionManager.selectedObjects.Count == 1)
        {
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
            UpdateMultipleDetails();
        }
    }
}
