using System.Collections.Generic;
using UnityEngine;

// This script will hide enemys when they are in object with this script
public class Viisibility : MonoBehaviour
{
    public class UnitData {
        public Unit unit;
        public Material material;
    }

    [SerializeField] private Material inVisibleMaterial;
    private List<UnitData> units = new ();

    private void OnTriggerEnter(Collider other) {
        var damagableScript = other.GetComponent<Damagable>();
        if (damagableScript == null) return;
        var attckScript = other.GetComponent<Attack>();

        if (attckScript == null) {
            HideUnit(damagableScript);
            return;
        }

        if (attckScript.target == null) {
            HideUnit(damagableScript);
            return;
        }

        attckScript.OnTarget += HandleOnTarget;        
    }

    private void OnTriggerExit(Collider other) {
        var damagableScript = other.GetComponent<Damagable>();
        if (damagableScript == null) return;
        MakeUnitVisible(damagableScript);
        RemoveUnit(other.GetComponent<Unit>());
    }

    private void MakeUnitVisible(Damagable damagable) {
        if (damagable != null && damagable.playerId != PlayerController.playerId) {
            var unitScript = damagable.GetComponent<Unit>();
            var unit = units.Find(u => u.unit == unitScript);

            if (unit == null) return;
            Debug.Log("Make unit visible " + unit.material.name);

            unitScript.bushes.Remove(gameObject);
            Debug.Log("Bushes count " + unitScript.bushes.Count);
            if (unitScript.bushes.Count > 0) return;
            unitScript.ShowUiPrefabs();
            unitScript.ChangeMaterial(unit.material);
            unitScript.isVisibile = true;
        }
    }

    private void RemoveUnit(Unit unit) {
        var unitData = units.Find(u => u.unit == unit);
        if (unitData == null) return;

        var attackScript = unitData.unit.GetComponent<Attack>();

        if (attackScript != null) attackScript.OnTarget -= HandleOnTarget;
        if (unitData != null) units.Remove(unitData);
    }

    private void HandleOnTarget(Damagable target, Unit currentUnit) {
        if (currentUnit == null) return;
        var damagableScript = currentUnit.GetComponent<Damagable>();

        if (target == null) {  
            HideUnit(damagableScript);
            return;
        }

        MakeUnitVisible(damagableScript);
    }

    private void HideUnit(Damagable damagable) {
        if (damagable != null && damagable.playerId != PlayerController.playerId) {
            var unitScript = damagable.GetComponent<Unit>();
            var attackScript = damagable.GetComponent<Attack>();

            if (attackScript != null) attackScript.OnTarget += HandleOnTarget;

            unitScript.bushes.Add(gameObject);
            
            units.Add(new UnitData { unit = unitScript, material = unitScript.originalMaterial });     
            unitScript.HideUiPrefabs();
            unitScript.ChangeMaterial(inVisibleMaterial);
            unitScript.isVisibile = false;
        }
    }
}
