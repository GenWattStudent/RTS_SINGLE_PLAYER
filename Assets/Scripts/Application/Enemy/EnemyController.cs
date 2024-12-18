using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Color enemyColor;
    public Material enemyMaterial;
    public static EnemyController Instance;
    public List<Unit> units = new();
    public ulong enemyId;
    public List<GameObject> unitPrefabs = new();
    public Vector3 spawnPosition = new Vector3(6, 0, 10f);
    [SerializeField] private float timeToSpawnEnemy = 20f;
    [SerializeField] private List<EnemySpawner> enemySpawners = new();
    private float timeToSpawnEnemyTimer = 0f;
    private bool isSpawning = false;

    private void SpawnUnits()
    {
        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 9; i++)
            {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();
                var unitMovement = unit.GetComponent<UnitMovement>();

                if (unitMovement != null)
                {
                    unitMovement.agent.enabled = true;
                }

                damagableScript.IsBot = true;
                unitScript.IsBot = true;
                unitScript.ChangeMaterial(enemyMaterial, true);
                units.Add(unitScript);

                spawnPosition += new Vector3(2f, 0, 0);
                unit.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    public void SpawnUnit(UnitSo unit, Vector3 spawnPosition)
    {
        var unitPrefab = unit.prefab;
        var unitScript = Instantiate(unitPrefab, spawnPosition, Quaternion.identity).GetComponent<Unit>();
        var damagableScript = unitScript.GetComponent<Damagable>();
        var unitMovement = unitScript.GetComponent<UnitMovement>();
        var selectable = unitScript.GetComponent<Selectable>();
        var resourceUsage = unitScript.GetComponent<ResourceUsage>();

        if (resourceUsage != null)
        {
            resourceUsage.enabled = false;
        }

        if (selectable != null)
        {
            selectable.enabled = false;
        }

        if (unitMovement != null)
        {
            Destroy(unitMovement);
            // unitScript.AddComponent<EnemyUnitMovement>();
        }

        // damagableScript.OwnerClientId = enemyId;
        // unitScript.OwnerClientId = enemyId;
        unitScript.ChangeMaterial(enemyMaterial, true);
        units.Add(unitScript);
    }

    private void UpdateTimeToSpawnEnemyText()
    {
        // display minutes and seconds
        var timer = timeToSpawnEnemy - timeToSpawnEnemyTimer;
        if (timer < 0) timer = 0;

        var minutes = Mathf.FloorToInt(timer / 60f);
        var seconds = Mathf.FloorToInt(timer % 60f);

        MiddleMessage.Instance.SetText($"Time to spawn enemies: {minutes:00}:{seconds:00}");
    }

    private void StartSpawners()
    {
        foreach (var enemySpawner in enemySpawners)
        {
            enemySpawner.StartSpawning();
        }
    }

    void Start()
    {
        enemyId = 1;
        Instance = this;
        SpawnUnits();
    }

    // Update is called once per frame
    void Update()
    {
        // if (isSpawning) return;
        // timeToSpawnEnemyTimer += Time.deltaTime;
        // // UpdateTimeToSpawnEnemyText();

        // if (timeToSpawnEnemyTimer >= timeToSpawnEnemy)
        // {
        //     timeToSpawnEnemyTimer = 0f;
        //     StartSpawners();
        //     isSpawning = true;
        //     MiddleMessage.Instance.HideTimerPanel();
        // }
    }
}
