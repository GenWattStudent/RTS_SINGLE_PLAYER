using System.Collections.Generic;
using UnityEngine;


public interface IConstruction
{
   GameObject prefab { get; set; }
   GameObject previewPrefab { get; set; }
   GameObject constructionManagerPrefab { get; set; }
}

[CreateAssetMenu(fileName = "New Building", menuName = "ScriptableObjects/Create Building")]
public class BuildingSo : DamagableSo, IConstruction
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
   public float income;
   public float incomeInterval;

   public UnitSo[] unitsToSpawn;
   public int maxBuildingCount;
   public ResourceSO resourceUsage;
   public GameObject levelUpEffect;

   public ResourceSO costResource;
   public int cost;

   public float sightAngle = 360f;
   public float sightRange = 4f;

   [Header("Building Prefabs")]
   [SerializeField] private GameObject _prefab;
   [SerializeField] private GameObject _previewPrefab;
   [SerializeField] private GameObject _constructionManagerPrefab;

   public GameObject prefab
   {
      get => _prefab;
      set => _prefab = value;
   }

   public GameObject previewPrefab
   {
      get => _previewPrefab;
      set => _previewPrefab = value;
   }

   public GameObject constructionManagerPrefab
   {
      get => _constructionManagerPrefab;
      set => _constructionManagerPrefab = value;
   }
}
