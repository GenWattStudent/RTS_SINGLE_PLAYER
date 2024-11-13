using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-150)]
public class VisibilityManager : NetworkBehaviour
{
    private Dictionary<NetworkObject, int> visibilityCounts = new Dictionary<NetworkObject, int>();

    private void Start()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.NetworkTickSystem.Tick += OnNetworkTick;
        }
    }

    private void OnNetworkTick()
    {
        UpdateVisibility();
    }

    private bool IsTargetHide(Unit unit, Unit enemyUnit)
    {
        var damagable = enemyUnit.GetComponent<Damagable>();
        var currentDamagable = unit.GetComponent<Damagable>();

        var ray = new Ray(currentDamagable.TargetPoint.position, damagable.TargetPoint.position - currentDamagable.TargetPoint.position);
        var hits = Physics.RaycastAll(ray, unit.unitSo.sightRange);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<Unit>() == enemyUnit)
            {
                return false; // Target is not hidden
            }
        }

        return true;
    }

    private bool IsUnitInSight(Unit unit, Unit enemyUnit)
    {
        var sightRange = unit.unitSo.sightRange;
        var sightAngle = unit.unitSo.sightAngle;

        var distance = Vector3.Distance(unit.transform.position, enemyUnit.transform.position);
        if (distance > sightRange) return false;

        var directionToUnit = (enemyUnit.transform.position - unit.transform.position).normalized;
        var angle = Vector3.Angle(unit.transform.forward, directionToUnit);
        if (angle > sightAngle / 2) return false;

        return !IsTargetHide(unit, enemyUnit);
    }

    public void Show(Unit unit)
    {
        var networkObject = unit.GetComponent<NetworkObject>();
        var IsNotServerUnit = NetworkManager.ServerClientId != OwnerClientId;

        if (IsNotServerUnit && !networkObject.IsNetworkVisibleTo(OwnerClientId))
        {
            networkObject.NetworkShow(OwnerClientId);
        }
        else if (!IsNotServerUnit)
        {
            unit.ShowUnit(unit);
        }
    }

    private void Hide(Unit unit)
    {
        var IsNotServerUnit = NetworkManager.ServerClientId != OwnerClientId;
        var networkObject = unit.GetComponent<NetworkObject>();

        if (IsNotServerUnit && networkObject.IsNetworkVisibleTo(OwnerClientId))
        {
            networkObject.NetworkHide(OwnerClientId);
        }
        else if (!IsNotServerUnit)
        {
            unit.HideUnit(unit);
        }
    }

    private void UpdateVisibility()
    {
        if (!RTSObjectsManager.Units.ContainsKey(OwnerClientId)) return;

        var playerUnits = RTSObjectsManager.Units[OwnerClientId];

        visibilityCounts.Clear();

        foreach (var unit in playerUnits)
        {
            if (unit == null) continue;

            foreach (var player in RTSObjectsManager.Units)
            {
                var playerController = NetworkManager.Singleton.ConnectedClients[player.Key].PlayerObject.GetComponent<PlayerController>();
                var unitPlayerController = NetworkManager.Singleton.ConnectedClients[unit.OwnerClientId].PlayerObject.GetComponent<PlayerController>();

                if (playerController.teamType.Value == unitPlayerController.teamType.Value) continue;

                foreach (var enemyUnit in player.Value)
                {
                    if (enemyUnit == null) continue;

                    if (IsUnitInSight(unit, enemyUnit))
                    {
                        var networkObject = enemyUnit.GetComponent<NetworkObject>();

                        if (!visibilityCounts.ContainsKey(networkObject))
                        {
                            visibilityCounts[networkObject] = 0;
                        }
                        visibilityCounts[networkObject] = 0;
                        visibilityCounts[networkObject]++;
                        networkObject.GetComponent<Unit>().isVisibile.Value = true;
                    }
                    else
                    {
                        var networkObject = enemyUnit.GetComponent<NetworkObject>();

                        if (!visibilityCounts.ContainsKey(networkObject))
                        {
                            visibilityCounts[networkObject] = 0;

                        }

                        networkObject.GetComponent<Unit>().isVisibile.Value = false;
                    }
                }
            }
        }

        // Show or hide units based on visibility counts
        foreach (var kvp in visibilityCounts)
        {
            if (kvp.Value > 0)
            {
                Show(kvp.Key.GetComponent<Unit>());
            }
            else
            {
                Hide(kvp.Key.GetComponent<Unit>());
            }
        }
    }
}