using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Guid playerId;
    public Color playerColor;
    public Material playerMaterial;
    public static PlayerController Instance;
    public List<Unit> units = new ();
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new ();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);

    // add unit event
    public event Action<Unit, List<Unit>> OnUnitChange;

    private void SpawnHero() {
        var heroInstance = Instantiate(hero, spawnPosition, Quaternion.identity);
        var damagableScript = heroInstance.GetComponent<Damagable>();
        var unitScript = heroInstance.GetComponent<Unit>();
        var unitMovement = heroInstance.GetComponent<UnitMovement>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        damagableScript.playerId = playerId;
        unitScript.playerId = playerId;
        unitScript.ChangeMaterial(playerMaterial);
        units.Add(unitScript);

        spawnPosition += new Vector3(2, 0 ,0);
    }

    public void AddUnit(Unit unit) {
        var damagableScript = unit.GetComponent<Damagable>();
        units.Add(unit);
        damagableScript.OnDead += () => {
            units.Remove(unit);
            OnUnitChange?.Invoke(unit, units);
        };

        OnUnitChange?.Invoke(unit, units);
    }

    private void SpawnUnits() {
        SpawnHero();
        foreach (var unitPrefab in unitPrefabs) {
            for (int i = 0; i < 2; i++) {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();
                var unitMovement = unit.GetComponent<UnitMovement>();

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                damagableScript.playerId = playerId;
                unitScript.playerId = playerId;
                unitScript.ChangeMaterial(playerMaterial);
                AddUnit(unitScript);

                spawnPosition += new Vector3(2, 0 ,0);
            }
        }
    }

    void Awake()
    {
        playerId = Guid.NewGuid();
        Instance = this;
    }

    void Start()
    {
        SpawnUnits();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
