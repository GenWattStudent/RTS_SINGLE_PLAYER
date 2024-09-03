using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RTSObjectsManager : NetworkBehaviour
{
    public List<Unit> Units { get; private set; } = new();
    public List<Building> Buildings { get; private set; } = new();
    public List<Unit> LocalPlayerUnits { get; private set; } = new();
    public List<Building> LocalPlayerBuildings { get; private set; } = new();
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddUnitServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var unit = no.GetComponent<Unit>();
            Units.Add(unit);

            Debug.Log($"Server have {Units.Count} units,");

            var damagableScript = unit.GetComponent<Damagable>();
            damagableScript.OnDead += () =>
            {
                RemoveUnitServerRpc(no);
            };

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { unit.OwnerClientId }
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
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveUnitServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var unit = no.GetComponent<Unit>();
            if (!Units.Contains(unit)) return;

            Units.Remove(unit);

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
        }
    }
}
