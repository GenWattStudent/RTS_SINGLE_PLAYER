using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitSo unitSo;
    public AttackableSo attackableSo;
    public Guid playerId;
    public Material unitMaterial;
    public Material originalMaterial;
    public bool shouldChangeMaterial = true;
    public List<GameObject> unitPrefabs = new ();
    public List<GameObject> unitUiPrefabs = new ();
    public List<GameObject> bushes = new ();
    private Damagable damagable;
    public bool isVisibile = true;

    public void ChangeMaterial(Material material, bool shouldChangeOriginalMaterial = false) {
        if (!shouldChangeMaterial) return;

        if (shouldChangeOriginalMaterial) {
            originalMaterial = material;
        }

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

    public void HideUiPrefabs() {
        foreach (var unitUiPrefab in unitUiPrefabs) {
            unitUiPrefab.SetActive(false);
        }
    }

    public void ShowUiPrefabs() {
        foreach (var unitUiPrefab in unitUiPrefabs) {
            unitUiPrefab.SetActive(true);
        }
    }

    private void Start() {
        damagable = GetComponent<Damagable>();

        if (damagable != null) {
            damagable.OnDead += HideUiPrefabs;        
        }
    }
 }
