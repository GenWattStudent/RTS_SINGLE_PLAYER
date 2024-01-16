using System.Collections.Generic;
using UnityEngine;

public class MaterialData {
    public Vector4 startValue;
    public Material material;
}

public class DissolveEffect : MonoBehaviour
{
    private List<MaterialData> materials = new ();
    private Vector4 startValue;
    public float dissolveSpeed = .3f;
    float startTime;
    // Start is called before the first frame update
    void Start()
    {
        Renderer meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null) {
            meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        };

        if (meshRenderer == null) return;

        var material = meshRenderer.material;
        startValue = material.GetVector("_DissolveOffest");

        if (materials != null) {
            materials.Add(new MaterialData() {
                startValue = startValue,
                material = material
            });
        }

        var renders = GetComponentsInChildren<MeshRenderer>();

        foreach (var render in renders) {
            materials.Add(new MaterialData() {
                startValue = render.material.GetVector("_DissolveOffest"),
                material = render.material
            });
        }

        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var materialData in materials)
        {
            // Calculate the new y value for _DissolveOffest
            float elapsedTime = Time.time - startTime;

            // Calculate the new y value for _DissolveOffest
            float newValue = Mathf.Lerp(materialData.startValue.y, 2f, elapsedTime * dissolveSpeed);
            // Set the new value
            materialData.material.SetVector("_DissolveOffest", new Vector4(0f, newValue, 0f, 0f));
        }
    }
}
