using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "ScriptableObjects/Unit", order = 1)]
public class UnitSo : DamagableSo
{
    public enum UnitType
    {
        Worker,
        Attacker,
        Healer,
        Commander
    }

    public UnitType type;
    public string unitName;
    public float speed;
    public float buildingDistance;
    public int cost;
    public ResourceSO costResource;
    public float spawnTime;
    public Sprite sprite;
    public GameObject prefab;
    public ushort spawnerLevelToUnlock;
    public ResourceSO resourceUsage;
    public int usage;
    public float usageInterval;
    public float sightAngle = 360f;
    public float sightRange = 16f;
}
