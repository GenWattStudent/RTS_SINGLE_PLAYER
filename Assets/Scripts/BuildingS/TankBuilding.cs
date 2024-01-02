using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankBuilding : MonoBehaviour, ISpawnerBuilding
{
    [SerializeField] private UnitSo unitToSpawn;
    [SerializeField] private Transform unitSpawnPoint;
    [SerializeField] private Transform unitMovePoint;
    [SerializeField] private BuildingSo building;
    private List<UnitSo> unitsQueue = new ();
    private float spawnTimer;
    private bool isUnitSpawning = false;
    private UnitSo currentSpawningUnit;
    private List<Animator> doorAnimators = new ();
    private Building buildingScript;
    public float totalSpawnTime {get; set;} = 0;

    private void Awake()
    {
        var animators = GetComponentsInChildren<Animator>();
        buildingScript = GetComponent<Building>();

        foreach (var animator in animators)
        {
            if (animator.gameObject.name == "Door") doorAnimators.Add(animator);
        }
    }

    private void OpenDoor()
    {
        foreach (var animator in doorAnimators)
        {
            animator.Play("OpenDoor", 0, 0);
        }
    }

    private void CloseDoor()
    {
        foreach (var animator in doorAnimators)
        {
            animator.SetBool("isOpen", false);
        }
    }

    IEnumerator<WaitForSeconds> CloseDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseDoor();
    }

    private void InstantiateUnit()
    {
        if (!UnitCountManager.Instance.CanSpawnUnit()) return;
        UIStorage.Instance.DecreaseResource(unitToSpawn.costResource, unitToSpawn.cost);

        var agent = unitToSpawn.prefab.GetComponent<NavMeshAgent>();
        agent.enabled = false;

        GameObject unitInstance = Instantiate(unitToSpawn.prefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        var unitScript = unitInstance.GetComponent<Unit>();
        var DamagableScript = unitInstance.GetComponent<Damagable>();
        
        if (DamagableScript != null) unitInstance.GetComponent<Damagable>().playerId = PlayerController.Instance.playerId;
        unitScript.playerId = PlayerController.Instance.playerId;
        unitScript.ChangeMaterial(PlayerController.Instance.playerMaterial, true);   

        if (unitMovePoint != null) {
            var unitMovement = unitInstance.GetComponent<UnitMovement>();
            if (unitMovement == null) return;

            unitMovement.SetDestinationAfterSpawn(unitMovePoint.position);
        }

        PlayerController.Instance.AddUnit(unitScript);

        OpenDoor();
        // calculate time when unit will be in unit move point
        float timeToMove = Vector3.Distance(unitSpawnPoint.position, unitMovePoint.position) / unitScript.unitSo.speed;
        StartCoroutine(CloseDoorAfterDelay(timeToMove));
    }

    public void AddUnitToQueue(UnitSo unit)
    {
        unitsQueue.Add(unit);
    }

    private void StartQueue()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning && UIStorage.Instance.HasEnoughResource(unitsQueue[0].costResource, unitsQueue[0].cost))
        {
            spawnTimer = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime;
            totalSpawnTime = spawnTimer;
            Debug.Log($"StartQueue {spawnTimer}");
            currentSpawningUnit = unitsQueue[0];
            isUnitSpawning = true;
        }
    }

    private void SpawnUnit()
    {
        if (spawnTimer > 0) return;
    
        if (unitsQueue.Count > 0)
        {
            unitToSpawn = unitsQueue[0];
            InstantiateUnit();
            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            currentSpawningUnit = null;
            StartQueue();
        }
    }

    private void UpdateScreen() {
        var screenController = GetComponentInChildren<ScreenController>();

        if (screenController == null) return;

        if (currentSpawningUnit != null) {
            screenController.SetProgresBar(spawnTimer, totalSpawnTime);
        } else {
            screenController.SetProgresBar(0, 0);
        }
    }

    private void Update() {
        spawnTimer -= Time.deltaTime;
        UpdateScreen();
        StartQueue();
        SpawnUnit();
    }

    public float GetSpawnTimer()
    {
        if (spawnTimer < 0) return 0;
        return spawnTimer;
    }

    public UnitSo GetCurrentSpawningUnit()
    {
        return currentSpawningUnit;
    }

    public int GetUnitQueueCountByName(string unitName)
    {
        return unitsQueue.FindAll(unit => unit.unitName == unitName).Count;
    }

    private void OnDestroy()
    {
        if (UIUnitManager.Instance.currentBuilding != this) return;
        UIUnitManager.Instance.ClearTabs();
    }
}
