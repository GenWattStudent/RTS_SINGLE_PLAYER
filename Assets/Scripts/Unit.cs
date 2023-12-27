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
    public List<GameObject> unitPrefabs = new ();

    private float effectDuration;
    public float effectTimer = 0;
    public float effectValue;
    public string effectKey;

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

    public void StartShaderValues(string key, float value, float duration = 1f) {
        if (unitMaterial == null) return;

        effectKey = key;
        effectValue = value;
        effectDuration = duration;
        effectTimer = 0f;
    } 

    public void UpdateShaderValues(string key, float value, float duration) {
        if (effectKey == key) {
            effectTimer += Time.deltaTime;
            float lerpValue = Mathf.Lerp(0, value, effectTimer / duration);

            int index = 0;
            foreach (var unitPrefab in unitPrefabs) {
                if (index != 0) return;
                var renderer = unitPrefab.GetComponent<Renderer>();
                renderer.material.SetVector(key, new Vector4(0, lerpValue, 0, 0));
            }
        }
    }

    private void Update() {
        if (effectKey != null) {
            UpdateShaderValues(effectKey, effectValue, effectDuration);
        }

        if (effectTimer >= effectDuration) {
            effectKey = null;
            effectTimer = 0f;
        }
    }
 }
