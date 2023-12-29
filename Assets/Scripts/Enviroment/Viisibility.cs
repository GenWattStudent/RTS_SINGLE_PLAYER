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

        HideUnit(damagableScript);
    }

    private void OnTriggerExit(Collider other) {
        var damagableScript = other.GetComponent<Damagable>();

        MakeUnitVisible(damagableScript);
        RemoveUnit(other.GetComponent<Unit>());
    }

    private void MakeUnitVisible(Damagable damagable) {
        if (damagable != null && damagable.playerId != PlayerController.Instance.playerId) {
            var unitScript = damagable.GetComponent<Unit>();
            var unit = units.Find(u => u.unit == unitScript);

            if (unit == null) return;
            Debug.Log("Make unit visible " + unit.material.name);
            unitScript.ChangeMaterial(unit.material);
        }
    }

    private void RemoveUnit(Unit unit) {
        var unitData = units.Find(u => u.unit == unit);

        if (unitData != null) units.Remove(unitData);

        var attackScript = unit.GetComponent<Attack>();

        if (attackScript != null) attackScript.OnTarget -= HandleOnTarget;
    }

    private void HandleOnTarget(Damagable target, Unit currentUnit) {
        var damagableScript = currentUnit.GetComponent<Damagable>();

        if (target == null) {  
            HideUnit(damagableScript);
            return;
        }

        MakeUnitVisible(damagableScript);
    }

    private void HideUnit(Damagable damagable) {
        if (damagable != null && damagable.playerId != PlayerController.Instance.playerId) {
            var unitScript = damagable.GetComponent<Unit>();
            var attackScript = damagable.GetComponent<Attack>();

            if (attackScript != null) attackScript.OnTarget += HandleOnTarget;

            units.Add(new UnitData { unit = unitScript, material = unitScript.unitMaterial });     
            unitScript.ChangeMaterial(inVisibleMaterial);
        }
    }
}
