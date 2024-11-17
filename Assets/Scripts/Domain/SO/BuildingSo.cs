using System.Collections.Generic;
using UnityEngine;

public interface IConstruction
{
   GameObject Prefab { get; }
   GameObject PreviewPrefab { get; }
   GameObject ConstructionManagerPrefab { get; }
}

[CreateAssetMenu(fileName = "New Building", menuName = "RTS/Create Building")]
public class BuildingSo : DamagableSo, IConstruction
{
   public enum BuildingType
   {
      Economy,
      Military,
      Defanse
   }

   [Header("General")]
   public string buildingName;
   public string description;
   public BuildingType type;
   public Sprite sprite;

   [Header("Income")]
   public float income;
   public float incomeInterval;
   public ResourceSO incomeResource;

   [Header("Spawner")]
   public List<UnitSo> unitsToSpawn;

   [Header("Other")]
   public int maxBuildingCount;
   public GameObject levelUpEffect;

   [Header("Usage")]
   public ResourceSO resourceUsage;
   public int usage;
   public float usageInterval;

   [Header("Cost")]
   public ResourceSO costResource;
   public int cost;

   [Header("Sight")]
   public float sightAngle = 360f;
   public float sightRange = 4f;

   [Header("Construction")]
   public GameObject Prefab;
   public GameObject PreviewPrefab;
   public GameObject ConstructionManagerPrefab;

   GameObject IConstruction.Prefab => Prefab;
   GameObject IConstruction.PreviewPrefab => PreviewPrefab;
   GameObject IConstruction.ConstructionManagerPrefab => ConstructionManagerPrefab;
}
