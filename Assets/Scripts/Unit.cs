using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitSo unitSo;
    public AttackableSo attackableSo;
    public Guid playerId;
    public Material unitMaterial;
    public bool shouldChangeMaterial = true;
    [SerializeField] private List<GameObject> unitPrefabs = new ();

    public void ChangeMaterial(Material material) {
        if (!shouldChangeMaterial) return;
        unitMaterial = material;
        
        if (unitMaterial != null) {
            foreach (var unitPrefab in unitPrefabs) {
                var meshRenderer = unitPrefab.GetComponent<MeshRenderer>();
                if (meshRenderer == null) {
                    unitPrefab.GetComponent<SkinnedMeshRenderer>().material = unitMaterial;
                    continue;
                }
                meshRenderer.material = unitMaterial;
            }
        }
    }
}
