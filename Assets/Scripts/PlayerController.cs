using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Guid playerId;
    public Color playerColor;
    public Material playerMaterial;
    public static PlayerController Instance;
    public List<Unit> units = new ();
    [SerializeField] private List<GameObject> unitPrefabs = new ();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);

    private void SpawnUnits() {

        foreach (var unitPrefab in unitPrefabs) {
            for (int i = 0; i < 2; i++) {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();

                damagableScript.playerId = playerId;
                unitScript.playerId = playerId;
                unitScript.ChangeMaterial(playerMaterial);
                units.Add(unitScript);

                spawnPosition += new Vector3(2, 0 ,0);
            }
        }
    }

    void Start()
    {
        playerId = Guid.NewGuid();
        Instance = this;
        SpawnUnits();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
