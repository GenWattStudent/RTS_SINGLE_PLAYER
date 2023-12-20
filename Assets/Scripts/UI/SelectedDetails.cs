using UnityEngine;

public class SelectedDetails : MonoBehaviour
{
    [SerializeField] private RectTransform imageGameObject;
    [SerializeField] private RectTransform healthBarGameObject;
    private ProgresBar healthBar;
    private Stats stats;

    private void Awake()
    {
        healthBar = healthBarGameObject.GetComponent<ProgresBar>();
        stats = GetComponentInChildren<Stats>();
        healthBar.UpdateProgresBar(0, 0);
    }

    private void CreateHealthStat(Damagable damagable) {
        stats.CreateStat("Health", $"{damagable.health}/{damagable.damagableSo.health}");
    }

    private void CreateDamageStat(Unit unit) {
        // stats.CreateStat("Damage", $"{unit.unitSo.bulletSo.damage}");
    }

    private void CreateExpirenceStat(Damagable damagable) {
        stats.CreateStat("Expirence", $"{damagable.expirence}");
    }

    private void ClearStats() {
        stats.ClearStats();
        // healthBar.UpdateProgresBar(0, 0);
        imageGameObject.GetComponent<UnityEngine.UI.Image>().sprite = null;
    }

    private void UpdateUnitDetails(Unit unit, Damagable damagable)
    {
        if (unit.unitSo == null) return;
        CreateHealthStat(damagable);
        CreateDamageStat(unit);
        CreateExpirenceStat(damagable);
        imageGameObject.GetComponent<UnityEngine.UI.Image>().sprite = unit.unitSo.sprite;
        healthBar.UpdateProgresBar(damagable.health, damagable.damagableSo.health);
    }

    private void UpdateBuildingDetails(Selectable selectable)
    {
        Debug.Log("Update building details");
    }

    private void UpdateMultipleDetails()
    {
        Debug.Log("Update multiple details");
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

            if (unit != null)
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
