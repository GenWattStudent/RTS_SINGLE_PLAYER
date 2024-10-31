using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RTSObjectsManager : NetworkBehaviour
{
    public static Dictionary<ulong, List<Unit>> Units { get; private set; } = new();
    public static Dictionary<ulong, List<Building>> Buildings { get; private set; } = new();
    public List<Unit> LocalPlayerUnits = new();
    public List<Building> LocalPlayerBuildings = new();

    public event Action<Unit, List<Unit>> OnUnitChange;
    public event Action<Building, List<Building>> OnBuildingChange;

    [ServerRpc(RequireOwnership = false)]
    public void AddUnitServerRpc(NetworkObjectReference nor, ServerRpcParams serverRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var unit = no.GetComponent<Unit>();

            if (!Units.ContainsKey(no.OwnerClientId))
            {
                Units[no.OwnerClientId] = new List<Unit>();
            }
            Units[no.OwnerClientId].Add(unit);

            var damagableScript = unit.GetComponent<Damagable>();
            damagableScript.OnDead += (Damagable damagable) =>
            {
                RemoveUnitServerRpc(no);
            };

            AddUnitClientRpc(no, GetClientRpcParams(no.OwnerClientId));
        }
    }

    [ClientRpc]
    private void AddUnitClientRpc(NetworkObjectReference nor, ClientRpcParams clientRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            if (no.OwnerClientId != OwnerClientId) return;
            var unit = no.GetComponent<Unit>();
            var damagableScript = unit.GetComponent<Damagable>();
            LocalPlayerUnits.Add(unit);
            Debug.Log($"Client({unit.OwnerClientId}, {OwnerClientId}) have {LocalPlayerUnits.Count} units.");
            // playerController.AddUnit(unit);
            OnUnitChange?.Invoke(unit, LocalPlayerUnits);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveUnitServerRpc(NetworkObjectReference nor, ServerRpcParams serverRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var senderClientId = no.OwnerClientId;
            var unit = no.GetComponent<Unit>();
            if (!Units[senderClientId].Contains(unit)) return;

            Units[senderClientId].Remove(unit);

            RemoveUnitClientRpc(no, GetClientRpcParams(no.OwnerClientId));
        }
    }

    [ClientRpc]
    private void RemoveUnitClientRpc(NetworkObjectReference nor, ClientRpcParams clientRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var unit = no.GetComponent<Unit>();
            LocalPlayerUnits.Remove(unit);
            // playerController.RemoveUnit(unit);
            OnUnitChange?.Invoke(unit, LocalPlayerUnits);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddBuildingServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var building = no.GetComponent<Building>();

            if (!Buildings.ContainsKey(no.OwnerClientId))
            {
                Buildings[no.OwnerClientId] = new List<Building>();
            }
            Buildings[no.OwnerClientId].Add(building);

            var damagableScript = building.GetComponent<Damagable>();
            damagableScript.OnDead += (Damagable damagable) =>
            {
                RemoveBuildingServerRpc(no);
            };

            AddBuildingClientRpc(no, GetClientRpcParams(no.OwnerClientId));
        }
    }

    [ClientRpc]
    private void AddBuildingClientRpc(NetworkObjectReference nor, ClientRpcParams clientRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            if (no.OwnerClientId != OwnerClientId) return;
            var building = no.GetComponent<Building>();
            LocalPlayerBuildings.Add(building);
            Debug.Log($"Client({building.OwnerClientId}, {OwnerClientId}) have {LocalPlayerBuildings.Count} buildings.");
            // playerController.AddBuilding(building);
            OnBuildingChange?.Invoke(building, LocalPlayerBuildings);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveBuildingServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var senderClientId = no.OwnerClientId;
            var building = no.GetComponent<Building>();
            if (!Buildings[senderClientId].Contains(building)) return;

            Buildings[senderClientId].Remove(building);

            RemoveBuildingClientRpc(no, GetClientRpcParams(no.OwnerClientId));
        }
    }

    [ClientRpc]
    private void RemoveBuildingClientRpc(NetworkObjectReference nor, ClientRpcParams clientRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var building = no.GetComponent<Building>();
            LocalPlayerBuildings.Remove(building);
            // playerController.RemoveBuilding(building);
            OnBuildingChange?.Invoke(building, LocalPlayerBuildings);
        }
    }

    public bool IsMaxBuildingOfType(BuildingSo buildingSo)
    {
        int count = GetBuildingCountOfType(buildingSo);
        return count >= buildingSo.maxBuildingCount;
    }

    public int GetBuildingCountOfType(BuildingSo buildingSo)
    {
        int count = 0;

        foreach (var building in LocalPlayerBuildings)
        {
            if (building.buildingSo.buildingName == buildingSo.buildingName) count++;
        }

        return count;
    }

    private ClientRpcParams GetClientRpcParams(ulong clientId)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
    }
}