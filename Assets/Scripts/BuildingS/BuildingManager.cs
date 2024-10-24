using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    public BuildingSo SelectedBuilding { get; private set; }
    private GameObject previewPrefab;
    public float diffranceBetweenMaxAndMinHeight = 1f;
    public int heightRaysCount = 15;
    private Vector3[] hightPoints;

    private void Awake()
    {
        hightPoints = new Vector3[heightRaysCount * heightRaysCount];
    }

    private void GetHightPoints()
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
                hightPoints[index] = new Vector3(x, 100f, z);
                index++;
            }
        }
    }

    private bool IsFlatTerrain()
    {
        float maxHeight = 0;
        float minHeight = 0;

        foreach (var point in hightPoints)
        {
            var rayPosition = new Vector3(point.x, 100f, point.z);
            Ray ray = new Ray(rayPosition, Vector3.down);

            // if ray hit notthing then return false
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

        return Mathf.Abs(maxHeight - minHeight) <= diffranceBetweenMaxAndMinHeight;
    }

    private bool IsBuildingColliding()
    {
        bool isCollidingWithOtherUnits = false;

        foreach (var hightPoint in hightPoints)
        {
            var rayPosition = new Vector3(hightPoint.x, 100f, hightPoint.z);
            Ray ray = new Ray(rayPosition, Vector3.down);

            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Terrain")
                    && hit.collider.gameObject.layer != LayerMask.NameToLayer("Ghost")
                    && !hit.collider.gameObject.CompareTag("ForceField"))
                {
                    isCollidingWithOtherUnits = true;
                    break;
                }
            }
        }

        return isCollidingWithOtherUnits;
    }

    public void SetSelectedBuilding(BuildingSo building)
    {
        SelectedBuilding = building;
        if (previewPrefab) Destroy(previewPrefab);
    }

    private Vector3? GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, terrainLayer))
        {
            return hit.point;
        }
        else
        {
            return null;
        }
    }

    private void SetPopup()
    {
        var message = $"{SelectedBuilding.name} ({PlayerController.GetBuildingCountOfType(SelectedBuilding)}/{SelectedBuilding.maxBuildingCount})";
        MousePopup.Instance.SetText(message);
        MousePopup.Instance.Show();
    }

    private void SetValidMaterial()
    {
        var renderers = previewPrefab.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = validMaterial;
        }
    }

    private void SetInvalidMaterial()
    {
        var renderers = previewPrefab.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = invalidMaterial;
        }
    }

    private void BuildingPreview()
    {
        if (SelectedBuilding is null) return;
        GetHightPoints();
        var mousePosition = GetMouseWorldPosition();
        bool isValid = IsValidPosition();

        SetPopup();
        if (mousePosition != null)
        {
            if (isValid)
            {
                SetValidMaterial();
            }
            else
            {
                SetInvalidMaterial();
            }
        }

        if (mousePosition != null && previewPrefab != null) previewPrefab.transform.position = (Vector3)mousePosition;
    }

    private void PlaceBuilding(Vector3 position)
    {
        if (!UIStorage.Instance.HasEnoughResource(SelectedBuilding.costResource, SelectedBuilding.cost)) return;
        UIStorage.Instance.DecreaseResource(SelectedBuilding.costResource, SelectedBuilding.cost);

        var newBuilding = Instantiate(SelectedBuilding.constructionManagerPrefab, position, SelectedBuilding.constructionManagerPrefab.transform.rotation);
        var damagableScript = newBuilding.GetComponent<Damagable>();
        var stats = newBuilding.GetComponent<Stats>();

        stats.AddStat(StatType.Health, 1);
        damagableScript.OwnerClientId = PlayerController.OwnerClientId;

        var unitScript = newBuilding.GetComponent<Unit>();
        unitScript.OwnerClientId = PlayerController.OwnerClientId;
        PlayerController.AddBuilding(newBuilding.GetComponent<Building>());
    }

    private void PlaceBuilding()
    {
        if (Input.GetMouseButtonDown(0) && SelectedBuilding != null)
        {
            if (!IsValidPosition())
            {
                InfoBox.Instance.AddError("You cant place building here!");
                return;
            };

            if (previewPrefab != null) PlaceBuilding(previewPrefab.transform.position);
            CancelBuilding();
        }
    }

    private bool IsValidPosition()
    {
        return !IsBuildingColliding() &&
        !PlayerController.IsMaxBuildingOfType(SelectedBuilding) &&
        IsFlatTerrain();
    }

    public void CancelBuilding()
    {
        if (previewPrefab != null) Destroy(previewPrefab);
        SelectedBuilding = null;
        UIBuildingManager.Instance.SetSelectedBuilding(null);
        MousePopup.Instance.Hide();
    }

    private void CheckSelectedBuilding()
    {
        var selectedBuilding = UIBuildingManager.Instance.GetSelectedBuilding();

        if (selectedBuilding != null && selectedBuilding != SelectedBuilding)
        {
            SetSelectedBuilding(selectedBuilding);
        }

        if (SelectedBuilding != null && previewPrefab == null)
        {
            var mousePosition = GetMouseWorldPosition();

            if (mousePosition != null)
            {
                previewPrefab = Instantiate(SelectedBuilding.previewPrefab);
                previewPrefab.transform.position = (Vector3)mousePosition;
                GetHightPoints();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (SelectedBuilding == null || previewPrefab == null) return;

        Gizmos.color = Color.blue;

        var collider = previewPrefab.GetComponent<BoxCollider>();
        var bounds = collider.bounds;

        // draw rectangle around the building
        Gizmos.DrawWireCube(previewPrefab.transform.position, bounds.size);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CancelBuilding();
        }

        CheckSelectedBuilding();
        BuildingPreview();
        PlaceBuilding();
    }
}
