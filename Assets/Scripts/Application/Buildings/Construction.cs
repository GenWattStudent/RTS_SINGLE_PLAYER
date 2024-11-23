using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class Construction : NetworkBehaviour, IWorkerConstruction
{
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject buildingInProgressPrefab;
    [SerializeField] private GameObject constructionPrefab;
    public List<Worker> buildingUnits = new();
    public IConstruction construction;

    private float constructionTimer = 0f;
    private bool isCurrentlyConstructing = false;
    private float buildingSpeed = 0f;
    private ProgresBar progresBar;
    private Stats stats;
    private SelectionManager selectionManager;

    public event Action OnFinshed;

    [ServerRpc(RequireOwnership = false)]
    public void DestroyConstructionServerRpc()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    public void AddWorker(Worker worker)
    {
        buildingUnits.Add(worker);
        buildingSpeed += worker.stats.GetStat(StatType.Damage);
        StartConstruction();
    }

    public void RemoveWorker(Worker worker)
    {
        if (buildingUnits.Contains(worker))
        {
            buildingUnits.Remove(worker);
            if (buildingSpeed > 0) buildingSpeed -= worker.stats.GetStat(StatType.Damage);
            if (buildingUnits.Count == 0)
            {
                StopConstruction();
            }
        }
    }

    public void RemoveWorkers()
    {
        for (int i = buildingUnits.Count - 1; i >= 0; i--)
        {
            var worker = buildingUnits[i];
            RemoveWorker(worker);
            worker.StopWorkerConstruction();
        }
    }

    public void StartConstruction()
    {
        isCurrentlyConstructing = true;
        ActivateBuildInProgressServerRpc();
    }

    public void StopConstruction()
    {
        isCurrentlyConstructing = false;
        ActivateConstructionBuildingServerRpc();
        RemoveWorkers();
    }

    private bool RemoveConstructionIfSelected()
    {
        if (selectionManager.selectedObjects.Count == 1)
        {
            var selectedConstruction = selectionManager.selectedObjects[0].GetComponent<Construction>();

            if (selectedConstruction == this)
            {
                var selectable = selectionManager.selectedObjects[0].GetComponent<Selectable>();
                selectionManager.Deselect(selectable);
                return true;
            }

            return false;
        }

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiateBuildingServerRpc()
    {
        var building = Instantiate(construction.Prefab, transform.position, transform.rotation);
        var no = building.GetComponent<NetworkObject>();
        var constructionNo = GetComponent<NetworkObject>();
        var damagable = building.GetComponent<Damagable>();
        var playerController = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();
        var rtsObjectManager = playerController.GetComponent<RTSObjectsManager>();
        var unit = transform.parent != null ? transform.parent.GetComponentInParent<Unit>() : null;

        damagable.teamType.Value = playerController.teamType.Value;
        no.SpawnWithOwnership(OwnerClientId);
        rtsObjectManager.RemoveBuildingServerRpc(constructionNo);
        rtsObjectManager.AddBuildingServerRpc(no);

        if (unit != null)
        {
            unit.IsUpgrading.Value = false;
            no.transform.SetParent(unit.transform);
        }

        InstantiateBuildingClientRpc(no);
        constructionNo.Despawn(true);
    }

    private void InstantiateBuilding()
    {
        // Finished building
        InstantiateBuildingServerRpc();
        OnFinshed?.Invoke();
    }

    [ClientRpc]
    private void InstantiateBuildingClientRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject networkObject))
        {
            var isRemoved = RemoveConstructionIfSelected();
            var selectable = networkObject.GetComponent<Selectable>();

            if (isRemoved) selectionManager.SelectBuilding(selectable);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateBuildInProgressServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ActivateBuildInProgressClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateConstructionBuildingServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ActivateConstructionBuildingClientRpc();
    }

    [ClientRpc]
    private void ActivateBuildInProgressClientRpc()
    {
        buildingInProgressPrefab.SetActive(true);
        constructionPrefab.SetActive(false);
    }

    [ClientRpc]
    private void ActivateConstructionBuildingClientRpc()
    {
        buildingInProgressPrefab.SetActive(false);
        constructionPrefab.SetActive(true);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        RemoveWorkers();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RemoveWorkers();
    }

    [ClientRpc]
    public void SetHealthClientRpc(float health, float maxHealth)
    {
        progresBar?.UpdateProgresBar(health, maxHealth);
    }

    private void Start()
    {
        progresBar = healthBar.GetComponent<ProgresBar>();
        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
        stats = GetComponent<Stats>();

        if (IsServer)
        {
            var health = stats.GetStat(StatType.Health);
            var maxHealth = stats.GetStat(StatType.MaxHealth);
            constructionTimer = health;

            SetHealthClientRpc(health, maxHealth);
        }
    }

    private void Update()
    {
        if (isCurrentlyConstructing && IsServer)
        {
            constructionTimer += buildingSpeed * Time.deltaTime;
            stats.SetStat(StatType.Health, Mathf.Floor(constructionTimer));

            var maxHealth = stats.GetStat(StatType.MaxHealth);

            SetHealthClientRpc(constructionTimer, maxHealth);

            if (constructionTimer >= maxHealth)
            {
                isCurrentlyConstructing = false;
                constructionTimer = 0;
                InstantiateBuilding();
            }
        }
    }
}
