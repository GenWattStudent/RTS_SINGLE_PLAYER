using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
            worker.StopConstruction(false);
        }
    }

    private bool RemoveConstructionIfSelected()
    {
        if (SelectionManager.selectedObjects.Count == 1)
        {
            var selectedConstruction = SelectionManager.selectedObjects[0].GetComponent<Construction>();

            if (selectedConstruction == this)
            {
                var selectable = SelectionManager.selectedObjects[0].GetComponent<Selectable>();
                SelectionManager.Deselect(selectable);
                return true;
            }

            return false;
        }

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiateBuildingServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var building = Instantiate(buildingSo.prefab, transform.position, Quaternion.identity);
        var no = building.GetComponent<NetworkObject>();
        var constructionNo = GetComponent<NetworkObject>();

        no.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        constructionNo.Despawn(true);
    }

    private void InstantiateBuilding()
    {
        // Finished building
        RemoveConstructionIfSelected();
        InstantiateBuildingServerRpc();
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

    void Start()
    {
        progresBar = healthBar.GetComponent<ProgresBar>();
        stats = GetComponent<Stats>();
        var health = stats.GetStat(StatType.Health);
        var maxHealth = stats.GetStat(StatType.MaxHealth);

        progresBar.UpdateProgresBar(health, maxHealth);
    }

    void Update()
    {
        if (isCurrentlyConstructing && IsOwner)
        {
            constructionTimer += buildingSpeed * Time.deltaTime;
            stats.SetStat(StatType.Health, Mathf.Floor(constructionTimer));

            var maxHealth = stats.GetStat(StatType.MaxHealth);

            progresBar.UpdateProgresBar(constructionTimer, maxHealth);

            if (constructionTimer >= maxHealth)
            {
                isCurrentlyConstructing = false;
                constructionTimer = 0;
                InstantiateBuilding();
            }
        }
    }
}
