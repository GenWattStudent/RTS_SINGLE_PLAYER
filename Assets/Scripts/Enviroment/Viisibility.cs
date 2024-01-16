using UnityEngine;

// This script will hide enemys when they are in object with this script
public class Viisibility : MonoBehaviour
{
    public class UnitData {
        public Unit unit;
        public Material material;
    }

    private void OnTriggerEnter(Collider other) {
        var damagableScript = other.GetComponent<Damagable>();
        if (damagableScript == null) return;

        HideUnit(damagableScript);
    }

    private void OnTriggerExit(Collider other) {
        var damagableScript = other.GetComponent<Damagable>();
        if (damagableScript == null) return;
        MakeUnitVisible(damagableScript);
    }

    private void MakeUnitVisible(Damagable damagable) {
        if (damagable != null && damagable.playerId != PlayerController.playerId) {
            var unitScript = damagable.GetComponent<Unit>();
            unitScript.bushes.Remove(gameObject);
        }
    }

    private void HideUnit(Damagable damagable) {
        if (damagable != null && damagable.playerId != PlayerController.playerId) {
            var unitScript = damagable.GetComponent<Unit>();
            unitScript.bushes.Add(gameObject);
        }
    }
}
