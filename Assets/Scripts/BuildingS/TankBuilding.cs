using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class TankBuilding : NetworkBehaviour, ISpawnerBuilding
{
    [SerializeField] private UnitSo unitToSpawn;
    [SerializeField] private Transform unitSpawnPoint;
    public Transform unitMovePoint;
    private List<UnitSo> unitsQueue = new();
    private NetworkVariable<float> spawnTimer = new NetworkVariable<float>(0);
    public NetworkVariable<float> totalSpawnTime = new NetworkVariable<float>(0);
    private bool isUnitSpawning = false;
    private UnitSo currentSpawningUnit;
    private Building buildingScript;

    private ResourceUsage resourceUsage;
    private PlayerController playerController;
    private UIUnitManager UIUnitManager;
    private UnitCountManager unitCountManager;
    private UIStorage uIStorage;
    public event Action<UnitSo, Unit> OnSpawnUnit;

    private void Awake()
    {
        buildingScript = GetComponent<Building>();
        resourceUsage = GetComponent<ResourceUsage>();
        playerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
        UIUnitManager = playerController.toolbar.GetComponent<UIUnitManager>();
        unitCountManager = playerController.toolbar.GetComponent<UnitCountManager>();
    }

    private Unit InstantiateUnit()
    {
        if (!unitCountManager.CanSpawnUnit())
        {
            InfoBox.Instance.AddError("You have reached unit limit");
            return null;
        };

        uIStorage.DecreaseResource(unitToSpawn.costResource, unitToSpawn.cost);

        var agent = unitToSpawn.prefab.GetComponent<NavMeshAgent>();
        agent.enabled = false;

        GameObject unitInstance = Instantiate(unitToSpawn.prefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        var unitScript = unitInstance.GetComponent<Unit>();

        unitScript.ChangeMaterial(playerController.playerData.playerMaterial, true);

        if (unitMovePoint != null)
        {
            var unitMovement = unitInstance.GetComponent<UnitMovement>();
            if (unitMovement != null) unitMovement.SetDestinationAfterSpawn(unitMovePoint.position);
        }

        return unitScript;
    }

    public void AddUnitToQueue(UnitSo unit)
    {
        unitsQueue.Add(unit);
        StartQueue();
    }

    private void StartQueue()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning)
        {
            spawnTimer.Value = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime;
            totalSpawnTime = spawnTimer;
            currentSpawningUnit = unitsQueue[0];
            isUnitSpawning = true;
        }
    }

    [ServerRpc]
    private void SpawnUnitServerRpc(ServerRpcParams rpcParams = default)
    {
        if (unitsQueue.Count > 0 && spawnTimer.Value < 0)
        {
            unitToSpawn = unitsQueue[0];
            var unit = InstantiateUnit();
            var no = unit.GetComponent<NetworkObject>();

            no.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            currentSpawningUnit = null;

            StartQueue();
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
            playerController.AddUnit(unit);
            OnSpawnUnit?.Invoke(unitToSpawn, unit);
        }
    }

    private void UpdateScreen()
    {
        var screenController = GetComponentInChildren<ScreenController>();

        if (screenController == null) return;

        if (currentSpawningUnit != null)
        {
            screenController.SetProgresBar(spawnTimer.Value, totalSpawnTime.Value);
        }
        else
        {
            screenController.SetProgresBar(0, 0);
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (currentSpawningUnit == null
            || !uIStorage.HasEnoughResource(currentSpawningUnit.costResource, currentSpawningUnit.cost)
            || resourceUsage.isInDebt) return;

        spawnTimer.Value -= Time.deltaTime;
        UpdateScreen();
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
