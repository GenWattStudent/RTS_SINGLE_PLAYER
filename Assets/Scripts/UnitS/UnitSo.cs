using UnityEngine;

[CreateAssetMenu(fileName = "Create Unit", menuName = "RTS/Unit")]
public class UnitSo : DamagableSo
{
    public enum UnitType
    {
        Worker,
        Attacker,
        Healer,
        Commander
    }
    [Header("General")]
    public UnitType type;
    public string unitName;
    public Sprite sprite;
    public GameObject prefab;

    [Header("Movement")]
    public float speed;
    public float acceleration;
    public float anguarRotation;

    [Header("Building")]
    public float buildingDistance;

    [Header("Cost")]
    public int cost;
    public ResourceSO costResource;

    [Header("Spawn")]
    public float spawnTime;
    public ushort spawnerLevelToUnlock;

    [Header("Usage")]
    public ResourceSO resourceUsage;
    public int usage;
    public float usageInterval;

    [Header("Sight")]
    public float sightAngle = 360f;
    public float sightRange = 16f;
}
