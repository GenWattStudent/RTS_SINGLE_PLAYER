using System.Collections.Generic;
using UnityEngine;

public class MaterialData
{
    public Vector4 startValue;
    public Material material;
}

public class DissolveEffect : MonoBehaviour
{
    public float dissolveSpeed = .3f;
    public float dissolveValue = 5f;

    private float startTime;
    private List<MaterialData> materials = new();
    private int disolveOffsetHash = Shader.PropertyToID("_DissolveOffest");

    private void Start()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var render in renderers)
        {
            materials.Add(new MaterialData()
            {
                startValue = render.material.GetVector(disolveOffsetHash),
                material = render.material
            });
        }

        startTime = Time.time;
    }

    private void UpdateMaterial()
    {
        foreach (var materialData in materials)
        {
            // Calculate the new y value for _DissolveOffest
            float elapsedTime = Time.time - startTime;

            // Calculate the new y value for _DissolveOffest
            float newValue = Mathf.Lerp(materialData.startValue.y, dissolveValue, elapsedTime * dissolveSpeed);

            // Set the new value
            materialData.material.SetVector(disolveOffsetHash, new Vector4(0f, newValue, 0f, 0f));
        }
    }

    private void Update()
    {
        UpdateMaterial();
    }
}
