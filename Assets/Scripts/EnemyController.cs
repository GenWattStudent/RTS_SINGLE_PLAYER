using System;
using System.Collections.Generic;
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

    private void SpawnUnits() {
        foreach (var unitPrefab in unitPrefabs) {
            for (int i = 0; i < 2; i++) {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();

                damagableScript.playerId = enemyId;
                unitScript.playerId = enemyId;
                unitScript.ChangeMaterial(enemyMaterial);
                units.Add(unitScript);

                spawnPosition += new Vector3(2f, 0 ,0);
            }
        }
    }

    void Start()
    {
        enemyId = Guid.NewGuid();
        Instance = this;
        SpawnUnits();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
