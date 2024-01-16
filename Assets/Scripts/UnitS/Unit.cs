using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Material inVisibleMaterial;
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
    private Attack attack;
    public bool isVisibile = true;
    private float visibleTimer = 0f;
    private float visibleInterval = 5f;

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
        attack = GetComponent<Attack>();
        visibleTimer = visibleInterval;

        if (damagable != null) {
            damagable.OnDead += HideUiPrefabs;        
        }
    }

    private void Update() {
        visibleTimer += Time.deltaTime;

        if (attack != null && attack.target != null) {
            ChangeMaterial(originalMaterial);
            visibleTimer = 0f;
            return;
        }

        if (visibleTimer >= visibleInterval) {
            return;
        }

        if (bushes.Count > 0) {
            isVisibile = false;
            ChangeMaterial(inVisibleMaterial);
        } else {
            isVisibile = true;
            ChangeMaterial(originalMaterial);
        }
    }
 }
