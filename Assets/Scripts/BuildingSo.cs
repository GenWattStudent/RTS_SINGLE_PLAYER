using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "ScriptableObjects/Create Building")]
public class BuildingSo : DamagableSo
{
    public enum BuildingType
    {
       Economy,
       Military,
       Defanse
    }

    public BuildingType type;
    public string buildingName;
    public string description;
    public Sprite sprite;
    public GameObject prefab;
    public GameObject validPrefab;
    public GameObject invalidPrefab;
    public GameObject constructionManagerPrefab;
    public int income;
    public float incomeInterval;
    public UnitSo[] unitsToSpawn;
    public int maxBuildingCount;
    public int cost;
}
