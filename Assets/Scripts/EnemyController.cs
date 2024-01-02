using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Color enemyColor;
    public Material enemyMaterial;
    public static EnemyController Instance;
    public List<Unit> units = new ();
    public Guid enemyId;
    public List<GameObject> unitPrefabs = new ();
    public Vector3 spawnPosition = new Vector3(6, 0, 10f);
    [SerializeField] private float timeToSpawnEnemy = 20f;
    [SerializeField] private TextMeshProUGUI timeToSpawnEnemyText;
    [SerializeField] private RectTransform timerPanel;
    [SerializeField] private List<EnemySpawner> enemySpawners = new ();
    private float timeToSpawnEnemyTimer = 0f;
    private bool isSpawning = false;

    private void SpawnUnits() {
        foreach (var unitPrefab in unitPrefabs) {
            for (int i = 0; i < 9; i++) {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();
                var unitMovement = unit.GetComponent<UnitMovement>();


                if (unitMovement != null) {
                    unitMovement.isReachedDestinationAfterSpawn = true;
                    unitMovement.agent.enabled = true;
                }

                damagableScript.playerId = enemyId;
                unitScript.playerId = enemyId;
                unitScript.ChangeMaterial(enemyMaterial, true);
                units.Add(unitScript);

                spawnPosition += new Vector3(2f, 0 ,0);
            }
        }
    }

    public void SpawnUnit(UnitSo unit, Vector3 spawnPosition) {
        var unitPrefab = unit.prefab;
        var unitScript = Instantiate(unitPrefab, spawnPosition, Quaternion.identity).GetComponent<Unit>();
        var damagableScript = unitScript.GetComponent<Damagable>();
        var unitMovement = unitScript.GetComponent<UnitMovement>();
    
        if (unitMovement != null) {
            Destroy(unitMovement);
            unitScript.AddComponent<EnemyUnitMovement>();
        }

        damagableScript.playerId = enemyId;
        unitScript.playerId = enemyId;
        unitScript.ChangeMaterial(enemyMaterial, true);
        units.Add(unitScript);
    }

    private void UpdateTimeToSpawnEnemyText() {
        // display minutes and seconds
        var timer = timeToSpawnEnemy - timeToSpawnEnemyTimer;
        if (timer < 0) timer = 0;

        var minutes = Mathf.FloorToInt(timer / 60f);
        var seconds = Mathf.FloorToInt(timer % 60f);

        timeToSpawnEnemyText.text = $"Time to spawn enemies: {minutes:00}:{seconds:00}";
    }

    private void StartSpawners() {
        foreach (var enemySpawner in enemySpawners) {
            enemySpawner.StartSpawning();
        }
    }

    void Start()
    {
        enemyId = Guid.NewGuid();
        Instance = this;
        // SpawnUnits();
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawning) return;
        timeToSpawnEnemyTimer += Time.deltaTime;
        UpdateTimeToSpawnEnemyText();

        if (timeToSpawnEnemyTimer >= timeToSpawnEnemy) {
            timeToSpawnEnemyTimer = 0f; 
            StartSpawners();
            isSpawning = true;
            timerPanel.gameObject.SetActive(false);
        }
    }
}
