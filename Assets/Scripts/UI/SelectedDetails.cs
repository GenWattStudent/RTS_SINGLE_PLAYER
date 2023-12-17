using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectedDetails : MonoBehaviour
{
    [SerializeField] private RectTransform statGameObject;
    [SerializeField] private RectTransform imageGameObject;
    [SerializeField] private RectTransform healthBarGameObject;
    [SerializeField] private RectTransform statsGameObject;
    private ProgresBar healthBar;
    private List<GameObject> statObjects = new List<GameObject>();

    private void Awake()
    {
        healthBar = healthBarGameObject.GetComponent<ProgresBar>();
        healthBar.UpdateProgresBar(0, 0);
    }

    private void Start()
    {
        SelectionManager.Instance.OnSelect += UpdateSelectedDetails;
    }

    private void OnDisable()
    {
        SelectionManager.Instance.OnSelect -= UpdateSelectedDetails;
    }

    private void CreateHealthStat(Damagable damagable) {
        var healthStat = Instantiate(statGameObject, statGameObject.transform);
        healthStat.SetParent(statsGameObject, false);
        var textMeshes = healthStat.GetComponentsInChildren<TextMeshProUGUI>();

        textMeshes[0].text = "Health";
        textMeshes[1].text = damagable.health + "/" + damagable.damagableSo.health;
        statObjects.Add(healthStat.gameObject);
        healthBar.UpdateProgresBar(damagable.health, damagable.damagableSo.health);
    }

    private void CreateDamageStat(Unit unit) {
        var damageStat = Instantiate(statGameObject, statGameObject.transform);
        damageStat.SetParent(statsGameObject, false);
        var textMeshes = damageStat.GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log("Create damage stat " + unit.unitSo.bulletSo.damage);
        textMeshes[0].text = "Damage";
        textMeshes[1].text = unit.unitSo.bulletSo.damage.ToString();
        statObjects.Add(damageStat.gameObject);
    }

    private void CreateExpirenceStat(Damagable damagable) {
        var expirenceStat = Instantiate(statGameObject, statGameObject.transform);
        expirenceStat.SetParent(statsGameObject, false);
        var textMeshes = expirenceStat.GetComponentsInChildren<TextMeshProUGUI>();

        textMeshes[0].text = "Expirence";
        textMeshes[1].text = damagable.expirence.ToString();
        statObjects.Add(expirenceStat.gameObject);
    }

    private void ClearStats() {
        foreach (var statObject in statObjects) {
            Destroy(statObject);
        }

        healthBar.UpdateProgresBar(0, 0);
        imageGameObject.GetComponent<UnityEngine.UI.Image>().sprite = null;
        statObjects.Clear();
    }

    private void UpdateUnitDetails(Unit unit, Damagable damagable)
    {
        Debug.Log("Update unit details");
        CreateHealthStat(damagable);
        CreateDamageStat(unit);
        CreateExpirenceStat(damagable);
        imageGameObject.GetComponent<UnityEngine.UI.Image>().sprite = unit.unitSo.sprite;
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
}
