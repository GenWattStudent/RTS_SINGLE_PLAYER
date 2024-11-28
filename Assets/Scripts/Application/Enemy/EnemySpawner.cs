using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int radius = 3;
    [SerializeField] private List<UnitSo> enemyUnitSoList;
    [SerializeField] private float spawnRate = 5f;
    public bool isSpawning = false;
    private float spawnTimer = 0f;

    public void StartSpawning() {
        isSpawning = true;
    }

    public void StopSpawning() {
        isSpawning = false;
    }

    private void SpawnEnemy() {
        if (enemyUnitSoList.Count == 0) return;

        var randomIndex = Random.Range(0, enemyUnitSoList.Count);
        var randomUnitSo = enemyUnitSoList[randomIndex];
        var randomPosition = Random.insideUnitSphere * radius;
        randomPosition += transform.position;
        randomPosition.y = 0;

        EnemyController.Instance.SpawnUnit(randomUnitSo, randomPosition);
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = spawnRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawning) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate) {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
