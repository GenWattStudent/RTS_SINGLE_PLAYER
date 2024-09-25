using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : NetworkBehaviour, ISpawnerBuilding
{
    [SerializeField] private Transform unitSpawnPoint;
    public Transform unitMovePoint;
    public NetworkVariable<float> totalSpawnTime = new(0);

    private bool isUnitSpawning = false;
    private UnitSo currentSpawningUnit;
    private Building buildingScript;
    private List<UnitSo> unitsQueue = new();
    private NetworkVariable<float> spawnTimer = new(0);
    private ResourceUsage resourceUsage;
    private PlayerController playerController;
    private RTSObjectsManager RTSObjectsManager;
    private UIUnitManager UIUnitManager;
    private UnitCountManager unitCountManager;
    private UIStorage uIStorage;

    public event Action<UnitSo, Unit> OnSpawnUnit;

    private void Start()
    {
        buildingScript = GetComponent<Building>();
        resourceUsage = GetComponent<ResourceUsage>();

        playerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
        RTSObjectsManager = playerController.GetComponent<RTSObjectsManager>();
        UIUnitManager = playerController.GetComponentInChildren<UIUnitManager>();
        unitCountManager = playerController.GetComponentInChildren<UnitCountManager>();
        uIStorage = playerController.GetComponentInChildren<UIStorage>();
    }

    private Unit InstantiateUnit()
    {
        if (!unitCountManager.CanSpawnUnit())
        {
            InfoBox.Instance.AddError("You have reached unit limit");
            return null;
        };

        uIStorage.DecreaseResource(currentSpawningUnit.costResource, currentSpawningUnit.cost);

        var agent = currentSpawningUnit.prefab.GetComponent<NavMeshAgent>();
        agent.enabled = false;

        GameObject unitInstance = Instantiate(currentSpawningUnit.prefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        var unitScript = unitInstance.GetComponent<Unit>();

        unitScript.ChangeMaterial(playerController.playerData.playerMaterial, true);

        if (unitMovePoint != null)
        {
            var unitMovement = unitInstance.GetComponent<UnitMovement>();
            if (unitMovement != null) unitMovement.SetDestinationAfterSpawn(unitMovePoint.position);
        }

        return unitScript;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddUnitToQueueServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        var unitSo = buildingScript.buildingSo.unitsToSpawn[index];
        unitsQueue.Add(unitSo);
        StartQueue();
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId };
        AddUnitToQueueClientRpc(index, clientRpcParams);
    }

    private void StartQueue()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning)
        {
            spawnTimer.Value = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime.Value;
            totalSpawnTime.Value = spawnTimer.Value;
            currentSpawningUnit = unitsQueue[0];
            isUnitSpawning = true;
        }
    }

    [ClientRpc]
    public void AddUnitToQueueClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        if (IsServer) return;

        var unitSo = buildingScript.buildingSo.unitsToSpawn[index];
        unitsQueue.Add(unitSo);
        currentSpawningUnit = unitsQueue[0];
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc()
    {
        if (unitsQueue.Count > 0 && spawnTimer.Value < 0)
        {
            var unit = InstantiateUnit();
            var no = unit.GetComponent<NetworkObject>();
            var damagable = unit.GetComponent<Damagable>();
            var playerController = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();

            damagable.teamType.Value = playerController.teamType.Value;
            no.SpawnWithOwnership(OwnerClientId);
            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            OnSpawnUnit?.Invoke(currentSpawningUnit, unit);
            currentSpawningUnit = null;

            StartQueue();
            RTSObjectsManager.AddUnitServerRpc(no);
            SpawnUnitClientRpc(no);
        }
    }

    [ClientRpc]
    private void SpawnUnitClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject no))
        {
            if (no.OwnerClientId != OwnerClientId) return;
            var unit = no.GetComponent<Unit>();
            RTSObjectsManager.LocalPlayerUnits.Add(unit);

            if (!IsServer)
            {
                if (unitsQueue.Count > 0)
                {
                    unitsQueue.RemoveAt(0);
                    isUnitSpawning = false;
                    if (unitsQueue.Count > 0) currentSpawningUnit = unitsQueue[0];
                }
                else
                {
                    currentSpawningUnit = null;
                }
            }
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (currentSpawningUnit == null
            || !uIStorage.HasEnoughResource(currentSpawningUnit.costResource, currentSpawningUnit.cost)
            || resourceUsage.isInDebt) return;

        spawnTimer.Value -= Time.deltaTime;
        SpawnUnitServerRpc();
    }

    public float GetSpawnTimer()
    {
        return spawnTimer.Value < 0 ? 0 : spawnTimer.Value;
    }

    public UnitSo GetCurrentSpawningUnit()
    {
        return currentSpawningUnit;
    }

    public int GetUnitQueueCountByName(string unitName)
    {
        return unitsQueue.FindAll(unit => unit.unitName == unitName).Count;
    }

    public override void OnDestroy()
    {
        if (UIUnitManager.currentBuilding != this) return;
        UIUnitManager.ClearTabs();
    }
}
