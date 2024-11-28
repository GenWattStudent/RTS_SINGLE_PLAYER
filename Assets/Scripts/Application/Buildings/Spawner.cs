using System;
using System.Collections.Generic;
using System.Linq;
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
    private UnitCountManager localUnitCountManager;
    private UIStorage uIStorage;
    private UIStorage localUIStorage;
    private BuildingLevelable buildingLevelable;
    private InfoBox infoBox;

    public event Action<UnitSo, Unit> OnSpawnUnit;
    public event Action<UnitSo> OnAddUnitToQueue;
    public event Action<UnitSo> OnCurrentUnitChange;

    private void Start()
    {
        buildingScript = GetComponent<Building>();
        resourceUsage = GetComponent<ResourceUsage>();
        buildingLevelable = GetComponent<BuildingLevelable>();

        var localPlayerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
        infoBox = localPlayerController.GetComponentInChildren<InfoBox>();
        localUnitCountManager = localPlayerController.GetComponentInChildren<UnitCountManager>();
        localUIStorage = localPlayerController.GetComponentInChildren<UIStorage>();

        if (IsServer)
        {
            playerController = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();
            unitCountManager = playerController.GetComponentInChildren<UnitCountManager>();
            uIStorage = playerController.GetComponentInChildren<UIStorage>();
        }
    }

    public bool IsValidIndex(int index) => index >= 0 && index < buildingScript.buildingSo.unitsToSpawn.Count;
    public bool HasValidLevel(int index) => buildingLevelable.level.Value >= buildingScript.buildingSo.unitsToSpawn[index].spawnerLevelToUnlock;

    #region Server Methods

    private Unit InstantiateUnit()
    {
        uIStorage.DecreaseResource(currentSpawningUnit.costResource, currentSpawningUnit.cost);

        GameObject unitInstance = Instantiate(currentSpawningUnit.prefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        var unitScript = unitInstance.GetComponent<Unit>();

        unitScript.ChangeMaterial(playerController.playerData.playerMaterial, true);

        return unitScript;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddUnitToQueueServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (!IsValidIndex(index) || !HasValidLevel(index)) return;

        var unitSo = buildingScript.buildingSo.unitsToSpawn[index];

        if (!uIStorage.HasEnoughResource(unitSo.costResource, unitSo.cost) || !unitCountManager.CanSpawnUnit(unitsQueue.Count + 1)) return;

        unitsQueue.Add(unitSo);
        NotifyClientUnitAddedToQueue(index, rpcParams);
        StartQueueServer();
    }

    private void NotifyClientUnitAddedToQueue(int index, ServerRpcParams rpcParams)
    {
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId };
        AddUnitToQueueClientRpc(index, clientRpcParams);
    }

    private Vector3 FindFreePosition(Vector3 startPosition, float radius, int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
            randomDirection += startPosition;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                if (!Physics.CheckSphere(hit.position, 0.5f)) // Check if the position is free
                {
                    return hit.position;
                }
            }
        }

        return startPosition; // Return the original position if no free position is found
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc()
    {
        if (unitsQueue.Count > 0 && spawnTimer.Value < 0)
        {
            var unit = InstantiateUnit();
            if (unit == null) return;

            SetupUnitNetwork(unit);

            if (unitMovePoint != null)
            {
                var unitMovement = unit.GetComponent<UnitMovement>();
                if (unitMovement != null) unitMovement.MoveToServerRpc(FindFreePosition(unitMovePoint.position, 5, 25));
            }

            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            OnSpawnUnit?.Invoke(unit.unitSo, unit);
            CurrentUnitChange(null);

            StartQueueServer();
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

    private void StartQueueServer()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning)
        {
            InitializeSpawnTimer();
            StartQueue();
            StartQueueClientRpc(new ClientRpcParams { Send = { TargetClientIds = new ulong[] { OwnerClientId } } });
        }
    }

    private void StartQueue()
    {
        CurrentUnitChange(unitsQueue[0]);
        isUnitSpawning = true;
    }

    [ClientRpc]
    private void StartQueueClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StartQueue();
    }

    private void InitializeSpawnTimer()
    {
        spawnTimer.Value = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime.Value;
        totalSpawnTime.Value = spawnTimer.Value;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (currentSpawningUnit == null || resourceUsage.isInDebt) return;

        spawnTimer.Value -= Time.deltaTime;
        SpawnUnitServerRpc();
    }

    #endregion

    #region Client Methods

    public void AddUnitToQueue(int index)
    {
        if (!IsValidIndex(index)) return;

        if (!localUnitCountManager.CanSpawnUnit(unitsQueue.Count + 1))
        {
            infoBox.AddError("You have reached unit limit");
            return;
        }

        if (!HasValidLevel(index))
        {
            infoBox.AddError("You need to upgrade building level");
            return;
        }

        if (!localUIStorage.HasEnoughResource(buildingScript.buildingSo.unitsToSpawn[index].costResource, buildingScript.buildingSo.unitsToSpawn[index].cost))
        {
            infoBox.AddError("Not enough resources");
            return;
        }

        AddUnitToQueueServerRpc(index);
    }

    [ClientRpc]
    public void AddUnitToQueueClientRpc(int index, ClientRpcParams clientRpcParams = default)
    {
        var unitSo = buildingScript.buildingSo.unitsToSpawn[index];
        Debug.Log($"Unit {unitSo.unitName} added to queue");
        if (!IsServer) unitsQueue.Add(unitSo);
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
            if (unitsQueue.Any()) CurrentUnitChange(unitsQueue[0]);
            else CurrentUnitChange(null);
        }
        else
        {
            isUnitSpawning = false;
            CurrentUnitChange(null);
        }

        Debug.Log($"Unit {no.GetComponent<Unit>().unitSo.unitName} spawned Queue count: {unitsQueue.Count}");
    }

    private void CurrentUnitChange(UnitSo unitSo)
    {
        currentSpawningUnit = unitSo;
        OnCurrentUnitChange?.Invoke(unitSo);
    }

    public UnitSo GetCurrentSpawningUnit() => currentSpawningUnit;
    public int GetUnitQueueCountByName(string unitName) => unitsQueue.FindAll(unit => unit.unitName == unitName).Count;

    public bool IsInsideSpawner(Vector3 position)
    {
        return GetComponent<BoxCollider>().bounds.Contains(position);
    }

    #endregion
}
