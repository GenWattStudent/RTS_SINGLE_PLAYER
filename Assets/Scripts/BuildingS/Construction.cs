using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class Construction : NetworkBehaviour
{
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject buildingInProgressPrefab;
    [SerializeField] private GameObject constructionPrefab;
    public List<Unit> buildingUnits = new();
    public BuildingSo buildingSo;

    private float constructionTimer = 0f;
    private bool isCurrentlyConstructing = false;
    private float buildingSpeed = 0f;
    private ProgresBar progresBar;
    private Stats stats;
    private SelectionManager selectionManager;

    public void AddWorker(Unit unit)
    {
        if (unit.unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Add(unit);
        buildingSpeed += unit.unitSo.attackDamage;
        Debug.Log("Add worker - building " + buildingUnits.Count + " - " + buildingSpeed);
        StartConstruction();
    }

    public void RemoveWorker(Unit unit)
    {
        if (unit.unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Remove(unit);
        if (buildingSpeed > 0) buildingSpeed -= unit.unitSo.attackDamage;
        if (buildingUnits.Count == 0)
        {
            StopConstruction();
        }
    }

    public void StartConstruction()
    {
        isCurrentlyConstructing = true;
        Debug.Log("Start construction - building");
        ActivateBuildInProgressServerRpc();
    }

    public void StopConstruction()
    {
        isCurrentlyConstructing = false;
        ActivateConstructionBuildingServerRpc();
        StopWorkersConstruction();
    }

    private void StopWorkersConstruction()
    {
        foreach (var unit in buildingUnits)
        {
            var worker = unit.GetComponent<Worker>();
            worker.StopConstructionServerRpc(false);
        }
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
        var building = Instantiate(buildingSo.prefab, transform.position, Quaternion.identity);
        var no = building.GetComponent<NetworkObject>();
        var constructionNo = GetComponent<NetworkObject>();

        no.SpawnWithOwnership(OwnerClientId);
        InstantiateBuildingClientRpc(no);
        constructionNo.Despawn(true);
    }

    private void InstantiateBuilding()
    {
        // Finished building
        InstantiateBuildingServerRpc();
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
        // Building with shader effect while someone is building it
        buildingInProgressPrefab.SetActive(true);
        constructionPrefab.SetActive(false);
    }

    [ClientRpc]
    private void ActivateConstructionBuildingClientRpc()
    {
        // Building with shader effect while someone is building it
        buildingInProgressPrefab.SetActive(false);
        constructionPrefab.SetActive(true);
    }

    public override void OnDestroy()
    {
        StopWorkersConstruction();
    }

    [ClientRpc]
    public void SetHealthClientRpc(float health, float maxHealth)
    {
        progresBar?.UpdateProgresBar(health, maxHealth);
    }

    void Start()
    {
        progresBar = healthBar.GetComponent<ProgresBar>();
        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
        stats = GetComponent<Stats>();

        Debug.Log("Construction Start " + OwnerClientId);

        if (IsServer)
        {
            var health = stats.GetStat(StatType.Health);
            var maxHealth = stats.GetStat(StatType.MaxHealth);
            constructionTimer = health;
            Debug.Log("Construction Start " + health + " " + maxHealth);
            SetHealthClientRpc(health, maxHealth);
        }
    }

    void Update()
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
