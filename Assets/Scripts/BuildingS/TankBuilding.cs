using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TankSpawner : MonoBehaviour, IPointerClickHandler, ISpawnerBuilding
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
        GameObject unitInstance = Instantiate(unitToSpawn.prefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        var unitScript = unitInstance.GetComponent<Unit>();
        var DamagableScript = unitInstance.GetComponent<Damagable>();
        
        if (DamagableScript != null) unitInstance.GetComponent<Damagable>().playerId = PlayerController.Instance.playerId;
        unitScript.playerId = PlayerController.Instance.playerId;
        unitScript.ChangeMaterial(PlayerController.Instance.playerMaterial);   

        var unitMovement = unitInstance.GetComponent<UnitMovement>();
        if (unitMovement == null) return;

        unitMovement.MoveTo(unitMovePoint.position);

        OpenDoor();
        // calculate time when unit will be in unit move point
        float timeToMove = Vector3.Distance(unitSpawnPoint.position, unitMovePoint.position) / unitScript.unitSo.speed;
        StartCoroutine(CloseDoorAfterDelay(timeToMove));
    }

    public void AddUnitToQueue(UnitSo unit)
    {
        unitsQueue.Add(unit);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        UIUnitManager.Instance.CreateUnitTabs(building, this, gameObject);
    }

    private void StartQueue()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning)
        {
            spawnTimer = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime;
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
        // var screenController = GetComponent<ScreenController>();

        // if (screenController == null) return;

        // if (currentSpawningUnit != null) {
        //     screenController.SetProgresBar(spawnTimer, currentSpawningUnit.spawnTime);
        // } else {
        //     screenController.SetProgresBar(0, 0);
        // }
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
        return unitsQueue.FindAll(unit => unit.name == unitName).Count;
    }

    private void OnDestroy()
    {
        if (UIUnitManager.Instance.currentBuilding != this) return;
        UIUnitManager.Instance.ClearTabs();
    }
}
