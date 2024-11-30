using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-150)]
public class VisibilityManager : NetworkBehaviour
{
    private HashSet<NetworkObject> visibleUnits = new HashSet<NetworkObject>();

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
        var sightRange = unit.unitSo != null ? unit.unitSo.sightRange : unit.GetComponent<Building>().buildingSo.sightRange;
        var hits = Physics.RaycastAll(ray, sightRange);

        Debug.DrawRay(ray.origin, ray.direction * sightRange, Color.red);
        bool isHidden = true;
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // if hits terrain dont see it
            var terrainLayer = LayerMask.NameToLayer("Terrain");

            if (hit.collider.gameObject.layer == terrainLayer)
            {
                isHidden = true;
                break;
            }

            if (hit.collider.gameObject.GetComponent<Unit>() == enemyUnit)
            {
                isHidden = false;
            }
        }

        return isHidden;
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

        var playerUnits = RTSObjectsManager.Objects[OwnerClientId];
        // Clear visibility states
        visibleUnits.Clear();

        foreach (var unit in playerUnits)
        {
            if (unit == null) continue;

            // Retrieve units in sight range using QuadTree
            var unitDamagable = unit.GetComponent<Damagable>();
            var sightRange = unit.unitSo != null ? unit.unitSo.sightRange : unit.GetComponent<Building>().buildingSo.sightRange;
            var unitsInRange = RTSObjectsManager.quadtree.FindEnemyUnitsInRange(
                unit.transform.position,
                sightRange,
                unitDamagable.teamType.Value);

            foreach (var enemyUnit in unitsInRange)
            {
                if (enemyUnit == null || visibleUnits.Contains(enemyUnit.GetComponent<NetworkObject>()) || IsTargetHide(unit, enemyUnit)) continue;

                var networkObject = enemyUnit.GetComponent<NetworkObject>();

                visibleUnits.Add(networkObject); // Mark as visible globally
            }
        }

        var currentPlayerController = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();
        // Loop through all enemy units units and update visibility
        foreach (var kvp in RTSObjectsManager.Objects)
        {
            if (currentPlayerController.teamType.Value == NetworkManager.Singleton.ConnectedClients[kvp.Key].PlayerObject.GetComponent<PlayerController>().teamType.Value) continue;

            foreach (var unit in kvp.Value)
            {
                if (unit == null) continue;

                var networkObject = unit.GetComponent<NetworkObject>();

                if (visibleUnits.Contains(networkObject))
                {
                    Show(unit);
                }
                else
                {
                    Hide(unit);
                }
            }
        }
    }
}