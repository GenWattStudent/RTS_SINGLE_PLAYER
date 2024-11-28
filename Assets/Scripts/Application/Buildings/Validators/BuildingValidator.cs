using UnityEngine;

public class BuildingValidator
{
    private int heightRaysCount;
    private LayerMask terrainLayer;
    private float differenceBetweenMaxAndMinHeight;
    private Vector3[] heightPoints;

    public BuildingValidator(int heightRaysCount, LayerMask terrainLayer, float differenceBetweenMaxAndMinHeight)
    {
        this.heightRaysCount = heightRaysCount;
        this.terrainLayer = terrainLayer;
        this.differenceBetweenMaxAndMinHeight = differenceBetweenMaxAndMinHeight;
        this.heightPoints = new Vector3[heightRaysCount * heightRaysCount];
    }

    public void GenerateHeightPoints(GameObject previewPrefab)
    {
        if (previewPrefab == null) return;

        var collider = previewPrefab.GetComponent<BoxCollider>();
        var bounds = collider.bounds;
        var rows = heightRaysCount;
        var cols = heightRaysCount;
        var index = 0;

        for (int i = 0; i < rows; i++)
        {
            var x = bounds.min.x + (bounds.size.x / rows) * i;
            for (int j = 0; j < cols; j++)
            {
                var z = bounds.min.z + (bounds.size.z / cols) * j;
                heightPoints[index] = new Vector3(x, 100f, z);
                index++;
            }
        }
    }

    public bool IsFlatTerrain()
    {
        float maxHeight = 0;
        float minHeight = 0;

        foreach (var point in heightPoints)
        {
            var rayPosition = new Vector3(point.x, 100f, point.z);
            Ray ray = new Ray(rayPosition, Vector3.down);

            // if ray hit nothing then return false
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, terrainLayer))
            {
                if (hit.point.y > maxHeight) maxHeight = hit.point.y;
                if (hit.point.y < minHeight) minHeight = hit.point.y;
            }
            else
            {
                return false;
            }
        }

        return Mathf.Abs(maxHeight - minHeight) <= differenceBetweenMaxAndMinHeight;
    }

    public bool IsBuildingColliding()
    {
        bool isValid = true;
        bool hasTerrain = false;

        foreach (var heightPoint in heightPoints)
        {
            var rayPosition = new Vector3(heightPoint.x, 100f, heightPoint.z);
            Ray ray = new Ray(rayPosition, Vector3.down);

            var raycastHits = Physics.RaycastAll(ray, float.MaxValue);
            foreach (var hit in raycastHits)
            {
                if (hit.collider.gameObject.CompareTag("ForceField"))
                {

                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                {
                    hasTerrain = true;
                }
                else if (hit.collider.gameObject.GetComponent<Collider>() != null && hit.collider.gameObject.layer != LayerMask.NameToLayer("FlatMap") && hit.collider.gameObject.layer != LayerMask.NameToLayer("Terrain") && hit.collider.gameObject.layer != LayerMask.NameToLayer("Ghost"))
                {
                    isValid = false;
                    break;
                }
                else if (hit.collider.gameObject.GetComponent<Unit>() != null)
                {
                    isValid = false;
                    break;
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid) break;
        }

        // The building is valid if it has terrain and either no force field or only force field
        return isValid && hasTerrain;
    }

    public bool IsValid()
    {
        return IsFlatTerrain() && IsBuildingColliding();
    }
}