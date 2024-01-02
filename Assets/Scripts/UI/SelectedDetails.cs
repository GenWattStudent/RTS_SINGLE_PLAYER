using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Selectable;

public class SelectedDetails : MonoBehaviour
{
    [SerializeField] private RectTransform imageGameObject;
    [SerializeField] private RectTransform healthBarGameObject;
    [SerializeField] private TextMeshProUGUI levelTextGameObject;
    [SerializeField] private RectTransform expirenceGameObject;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    private ProgresBar healthBar;
    private ProgresBar expirenceBar;
    private Stats stats;
    private Building building;

    private void Awake()
    {
        healthBar = healthBarGameObject.GetComponent<ProgresBar>();
        stats = GetComponentInChildren<Stats>();
        expirenceBar = expirenceGameObject.GetComponent<ProgresBar>();
        ActivateButtons(false);
    }

    private void OnEnable() {
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
        sellButton.onClick.AddListener(OnSellButtonClick);
    }

    private void OnDisable() {
        upgradeButton.onClick.RemoveListener(OnUpgradeButtonClick);
        sellButton.onClick.RemoveListener(OnSellButtonClick);
    }

    private void ActivateButtons(bool isActive) {
        upgradeButton.gameObject.SetActive(isActive);
        sellButton.gameObject.SetActive(isActive);
    }

    private void OnUpgradeButtonClick() {
        building.buildingLevelable.LevelUp();
    }

    private void OnSellButtonClick() {
        building.Sell();
    }

    private void CreateHealthStat(Damagable damagable) {
        stats.CreateStat("Health", $"{damagable.health}/{damagable.damagableSo.health}");
    }

    private void CreateDamageStat(AttackableSo attackableSo) {
        stats.CreateStat("Damage", $"{attackableSo.bulletSo.damage}");
    }

    private void CreateExpirenceStat(Damagable damagable) {
        stats.CreateStat("Expirence", $"{damagable.levelable.expirence}");
        expirenceBar.UpdateProgresBar(damagable.levelable.expirence, damagable.levelable.expirenceToNextLevel);
        levelTextGameObject.text = $"{damagable.levelable.level} LVL";
    }

    private void ClearStats() {
        stats.ClearStats();
        // healthBar.UpdateProgresBar(0, 0);
        imageGameObject.GetComponent<Image>().sprite = null;
        ActivateButtons(false);
    }

    private void UpdateUnitDetails(Unit unit, Damagable damagable)
    {
        CreateHealthStat(damagable);
        CreateDamageStat(unit.attackableSo);
        CreateExpirenceStat(damagable);
        imageGameObject.GetComponent<Image>().sprite = unit.unitSo.sprite;
        healthBar.UpdateProgresBar(damagable.health, damagable.damagableSo.health);
    }

    private void UpdateBuildingDetails(Selectable selectable)
    {
        var damagable = selectable.GetComponent<Damagable>();
        var building = selectable.GetComponent<Building>();
        CreateHealthStat(damagable);

        if (building != null) {
            imageGameObject.GetComponent<Image>().sprite = building.buildingSo.sprite;

            if (building.attackableSo != null) {
                CreateDamageStat(building.attackableSo);
            }

            healthBar.UpdateProgresBar(damagable.health, damagable.damagableSo.health);

            var construction = selectable.GetComponent<Construction>();

            if (construction != null) {
                this.building = building;
                upgradeButton.gameObject.SetActive(false);
                sellButton.gameObject.SetActive(true);
                return;
            }

            if (building.buildingLevelable != null && building.buildingLevelable.maxLevel > building.buildingLevelable.level) {
                levelTextGameObject.text = $"{building.buildingLevelable.level} LVL";
                this.building = building;
                ActivateButtons(true);
            } else {
                upgradeButton.gameObject.SetActive(false);
                sellButton.gameObject.SetActive(true);
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
        if (SelectionManager.Instance.selectedObjects.Count == 0) return;

        if (SelectionManager.Instance.selectedObjects.Count == 1)
        {
            var selectable = SelectionManager.Instance.selectedObjects[0];
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

    private void FixedUpdate()
    {
        UpdateSelectedDetails();
    }
}
