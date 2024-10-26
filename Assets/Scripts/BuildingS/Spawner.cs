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
    public NetworkVariable<float> spawnTimer = new(0);

    private bool isUnitSpawning = false;
    private UnitSo currentSpawningUnit;
    private Building buildingScript;
    private List<UnitSo> unitsQueue = new();
    private ResourceUsage resourceUsage;
    private PlayerController playerController;
    private UnitCountManager unitCountManager;
    private UIStorage uIStorage;
    private BuildingLevelable buildingLevelable;
    private InfoBox infoBox;

    public event Action<UnitSo, Unit> OnSpawnUnit;
    public event Action<UnitSo> OnAddUnitToQueue;
    public event Action<UnitSo> OnCurrentUnitChange;

    private void Start()
    {
        buildingScript = GetComponent<Building>();
        resourceUsage = GetComponent<ResourceUsage>();

        playerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
        infoBox = playerController.GetComponentInChildren<InfoBox>();
        unitCountManager = playerController.GetComponentInChildren<UnitCountManager>();
        uIStorage = playerController.GetComponentInChildren<UIStorage>();
        buildingLevelable = GetComponent<BuildingLevelable>();
    }

    public bool IsValidIndex(int index) => index >= 0 && index < buildingScript.buildingSo.unitsToSpawn.Count;
    public bool HasValidLevel(int index) => buildingLevelable.level.Value >= buildingScript.buildingSo.unitsToSpawn[index].spawnerLevelToUnlock;

    #region Server Methods

    private Unit InstantiateUnit()
    {
        if (!unitCountManager.CanSpawnUnit())
        {
            return null;
        }

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
        if (!IsValidIndex(index) || !HasValidLevel(index)) return;

        var unitSo = buildingScript.buildingSo.unitsToSpawn[index];

        unitsQueue.Add(unitSo);
        StartQueue();
        NotifyClientUnitAddedToQueue(index, rpcParams);
    }

    private void NotifyClientUnitAddedToQueue(int index, ServerRpcParams rpcParams)
    {
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId };
        AddUnitToQueueClientRpc(index, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc()
    {
        if (unitsQueue.Count > 0 && spawnTimer.Value < 0)
        {
            var unit = InstantiateUnit();
            if (unit == null) return;

            SetupUnitNetwork(unit);

            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            OnSpawnUnit?.Invoke(unit.unitSo, unit);
            CurrentUnitChange(null);

            StartQueue();
        }
    }

    private void SetupUnitNetwork(Unit unit)
    {
        var no = unit.GetComponent<NetworkObject>();
        var damagable = unit.GetComponent<Damagable>();
        var playerController = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();
        var RTSObjectsManager = playerController.GetComponent<RTSObjectsManager>();

        no.SpawnWithOwnership(OwnerClientId);
        damagable.teamType.Value = playerController.teamType.Value;
        RTSObjectsManager.AddUnitServerRpc(no);
        SpawnUnitClientRpc(no);
    }

    private void StartQueue()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning)
        {
            InitializeSpawnTimer();
            CurrentUnitChange(unitsQueue[0]);
            isUnitSpawning = true;
        }
    }

    private void InitializeSpawnTimer()
    {
        spawnTimer.Value = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime.Value;
        totalSpawnTime.Value = spawnTimer.Value;
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

    #endregion

    #region Client Methods

    public void AddUnitToQueue(int index)
    {
        if (IsValidIndex(index)) return;

        if (unitCountManager.CanSpawnUnit())
        {
            infoBox.AddError("You have reached unit limit");
            return;
        }

        if (!HasValidLevel(index))
        {
            infoBox.AddError("You need to upgrade building level");
            return;
        }

        AddUnitToQueueServerRpc(index);
    }

    [ClientRpc]
    public void AddUnitToQueueClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        var unitSo = buildingScript.buildingSo.unitsToSpawn[index];

        if (!IsServer) unitsQueue.Add(unitSo);
        CurrentUnitChange(unitsQueue[0]);
        OnAddUnitToQueue?.Invoke(unitSo);
    }

    [ClientRpc]
    private void SpawnUnitClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject no))
        {
            if (!IsServer && no.OwnerClientId == OwnerClientId)
            {
                HandleClientUnitSpawn(no);
            }
        }
    }

    private void HandleClientUnitSpawn(NetworkObject no)
    {
        OnSpawnUnit?.Invoke(no.GetComponent<Unit>().unitSo, no.GetComponent<Unit>());
        if (unitsQueue.Count > 0)
        {
            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            if (unitsQueue.Count > 0) CurrentUnitChange(unitsQueue[0]);
        }
        else
        {
            isUnitSpawning = false;
            CurrentUnitChange(null);
        }
    }

    private void CurrentUnitChange(UnitSo unitSo)
    {
        currentSpawningUnit = unitSo;
        OnCurrentUnitChange?.Invoke(unitSo);
    }

    public UnitSo GetCurrentSpawningUnit() => currentSpawningUnit;
    public int GetUnitQueueCountByName(string unitName) => unitsQueue.FindAll(unit => unit.unitName == unitName).Count;

    #endregion
}
