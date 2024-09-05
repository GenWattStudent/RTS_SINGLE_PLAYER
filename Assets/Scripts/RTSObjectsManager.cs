using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RTSObjectsManager : NetworkBehaviour
{
    public static Dictionary<ulong, List<Unit>> Units { get; private set; } = new();
    public List<Building> Buildings { get; private set; } = new();
    public List<Unit> LocalPlayerUnits { get; private set; } = new();
    public List<Building> LocalPlayerBuildings { get; private set; } = new();
    private PlayerController playerController;

    public static event Action<Unit, List<Unit>> OnUnitChange;
    public static event Action<Building, List<Building>> OnBuildingChange;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddUnitServerRpc(NetworkObjectReference nor, ServerRpcParams serverRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            Debug.Log("AddUnitServerRpc:  " + no.OwnerClientId);
            var unit = no.GetComponent<Unit>();

            if (!Units.ContainsKey(no.OwnerClientId))
            {
                Units[no.OwnerClientId] = new List<Unit>();
            }
            Units[no.OwnerClientId].Add(unit);

            var damagableScript = unit.GetComponent<Damagable>();
            damagableScript.OnDead += () =>
            {
                RemoveUnitServerRpc(no);
            };

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { no.OwnerClientId }
                }
            };

            AddUnitClientRpc(no, clientRpcParams);
        }
    }

    [ClientRpc]
    private void AddUnitClientRpc(NetworkObjectReference nor, ClientRpcParams clientRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var unit = no.GetComponent<Unit>();
            LocalPlayerUnits.Add(unit);
            Debug.Log($"Client({unit.OwnerClientId}) have {LocalPlayerUnits.Count} units,");
            // playerController.AddUnit(unit);
            OnUnitChange?.Invoke(unit, LocalPlayerUnits);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveUnitServerRpc(NetworkObjectReference nor, ServerRpcParams serverRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var senderClientId = serverRpcParams.Receive.SenderClientId;
            var unit = no.GetComponent<Unit>();
            if (!Units[senderClientId].Contains(unit)) return;

            Units[senderClientId].Remove(unit);

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { no.OwnerClientId }
                }
            };

            RemoveUnitClientRpc(no, clientRpcParams);
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
            Buildings.Add(building);

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { building.OwnerClientId }
                }
            };

            AddBuildingClientRpc(no, clientRpcParams);
        }
    }

    [ClientRpc]
    private void AddBuildingClientRpc(NetworkObjectReference nor, ClientRpcParams clientRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var building = no.GetComponent<Building>();
            LocalPlayerBuildings.Add(building);
            // playerController.AddBuilding(building);
            OnBuildingChange?.Invoke(building, LocalPlayerBuildings);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveBuildingServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var building = no.GetComponent<Building>();
            if (!Buildings.Contains(building)) return;

            Buildings.Remove(building);

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { building.OwnerClientId }
                }
            };

            RemoveBuildingClientRpc(no, clientRpcParams);
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
}
