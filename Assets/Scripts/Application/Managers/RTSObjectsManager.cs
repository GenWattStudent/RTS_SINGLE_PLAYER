using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RTSObjectsManager : NetworkBehaviour
{
    public static Dictionary<ulong, List<Unit>> Objects { get; private set; } = new();
    public static Dictionary<ulong, List<Unit>> Units { get; private set; } = new();
    public static Dictionary<ulong, List<Building>> Buildings { get; private set; } = new();
    public static QuadTree quadtree = new QuadTree(0, new Rect(0, 0, 250, 250));

    public List<Unit> LocalPlayerUnits = new();
    public List<Building> LocalPlayerBuildings = new();

    public event Action<Unit, List<Unit>> OnUnitChange;
    public event Action<Building, List<Building>> OnBuildingChange;

    #region Draw Quadtree
    private void OnDrawGizmos()
    {
        quadtree.DrawGizmos();
    }
    #endregion

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

            OnClientConnected(OwnerClientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            // NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        Units[clientId] = new List<Unit>();
        Buildings[clientId] = new List<Building>();
        Objects[clientId] = new List<Unit>();
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // quadtree.Clear();
        Units.Remove(clientId);
        Buildings.Remove(clientId);
        Objects.Remove(clientId);
    }
    private void HandleUnitDeath(Damagable damagable)
    {
        RemoveUnitServerRpc(damagable.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddUnitServerRpc(NetworkObjectReference nor, ServerRpcParams serverRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            Debug.Log("AddUnitServerRpc");
            var unit = no.GetComponent<Unit>();
            Units[no.OwnerClientId].Add(unit);
            Objects[no.OwnerClientId].Add(unit);
            quadtree.Insert(unit);

            unit.GetComponent<Damagable>().OnDead += HandleUnitDeath;
        }
    }

    public void AddLocalUnit(Unit unit)
    {
        LocalPlayerUnits.Add(unit);
        OnUnitChange?.Invoke(unit, LocalPlayerUnits);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveUnitServerRpc(NetworkObjectReference nor, ServerRpcParams serverRpcParams = default)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var unit = no.GetComponent<Unit>();
            if (!Units[no.OwnerClientId].Contains(unit)) return;

            quadtree.Remove(unit);
            unit.GetComponent<Damagable>().OnDead -= HandleUnitDeath;
            Units[no.OwnerClientId].Remove(unit);
            Objects[no.OwnerClientId].Remove(unit);
        }
    }

    public void RemoveLocalUnit(Unit unit)
    {
        LocalPlayerUnits.Remove(unit);
        OnUnitChange?.Invoke(unit, LocalPlayerUnits);
    }

    private void HandleBuildingDeath(Damagable damagable)
    {
        RemoveBuildingServerRpc(damagable.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddBuildingServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var building = no.GetComponent<Building>();
            Buildings[no.OwnerClientId].Add(building);
            Objects[no.OwnerClientId].Add(building.GetComponent<Unit>());
            quadtree.Insert(building.GetComponent<Unit>());

            building.GetComponent<Damagable>().OnDead += HandleBuildingDeath;
        }
    }

    public void AddLocalBuilding(Building building)
    {
        LocalPlayerBuildings.Add(building);
        OnBuildingChange?.Invoke(building, LocalPlayerBuildings);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveBuildingServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var building = no.GetComponent<Building>();
            if (!Buildings[no.OwnerClientId].Contains(building)) return;

            quadtree.Remove(building.GetComponent<Unit>());
            building.GetComponent<Damagable>().OnDead -= HandleBuildingDeath;
            Buildings[no.OwnerClientId].Remove(building);
            Objects[no.OwnerClientId].Remove(building.GetComponent<Unit>());
        }
    }

    public void RemoveLocalBuilding(Building building)
    {
        LocalPlayerBuildings.Remove(building);
        OnBuildingChange?.Invoke(building, LocalPlayerBuildings);
    }

    public bool IsMaxBuildingOfType(BuildingSo buildingSo)
    {
        return GetBuildingCountOfType(buildingSo) >= buildingSo.maxBuildingCount;
    }

    public int GetBuildingCountOfType(BuildingSo buildingSo)
    {
        return LocalPlayerBuildings.FindAll(b => b.buildingSo.buildingName == buildingSo.buildingName).Count;
    }
}