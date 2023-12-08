using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask terrainLayer;
    public BuildingSo SelectedBuilding { get; private set; }
    private PlaceableBuilding placeableBuilding;
    private GameObject previewPrefab;
    private static BuildingSystem instance;
    public static BuildingSystem Instance { get { return instance; } }
    private bool wasValid = false;
    [SerializeField] private int numberOfRays = 25;

    private void Awake() {
        instance = this;
        camera = Camera.main;
    }
        bool IsBuildingPlacementValid(Vector3 center)
    {
        // Calculate the positions for rays around the building's base
        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * 360f / numberOfRays;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            Ray ray = new Ray(center, rayDirection);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);

            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayer))
            {
                Debug.Log("Hit point");
                // Check if the terrain slope is within an acceptable range
                if (!IsTerrainFlat(hit.point))
                {
                    return false; // Terrain is too steep at this point
                }
            }
            else
            {
                Debug.Log("No hit point");
                return false; // Ray didn't hit terrain, consider it invalid
            }
        }

        return true; // All rays found flat terrain
    }

    bool IsTerrainFlat(Vector3 point)
    {
        // Sample the terrain normal at the specified point
        Terrain terrain = Terrain.activeTerrain;
        Debug.Log(terrain);
        if (terrain != null)
        {
            Vector3 terrainNormal = terrain.terrainData.GetInterpolatedNormal(
                point.x / terrain.terrainData.size.x,
                point.z / terrain.terrainData.size.z
            );

            // You can adjust the slope threshold based on your requirements
            float slopeThreshold = 0.3f; // Example threshold

            // Check if the terrain slope is within an acceptable range
            Debug.Log(Vector3.Dot(terrainNormal, Vector3.up));
            return Vector3.Dot(terrainNormal, Vector3.up) > 1 - slopeThreshold;
        }

        return false;
    }

    public void SetSelectedBuilding(BuildingSo building) {
        SelectedBuilding = building;
        if (previewPrefab) Destroy(previewPrefab);
    }

    private Vector3? GetMouseWorldPosition() {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, terrainLayer)) {
            return hit.point;
        } else {
            return null;
        }
    }

    private void BuildingPreview() {
        if (SelectedBuilding is null) return;

        var mousePosition = GetMouseWorldPosition();
        bool isValid = IsValidPosition();

        if (isValid != wasValid && mousePosition != null) {
            if (previewPrefab != null) Destroy(previewPrefab);
            if (isValid) {
                previewPrefab = Instantiate(SelectedBuilding.validPrefab);
            } else {
                previewPrefab = Instantiate(SelectedBuilding.invalidPrefab);
            }

            placeableBuilding = previewPrefab.GetComponent<PlaceableBuilding>();
            wasValid = isValid;
        }

        if (mousePosition != null && previewPrefab != null) previewPrefab.transform.position = (Vector3)mousePosition;
    }

    private void PlaceBuilding(Vector3 position) {
        if (SelectedBuilding == null) return;
        var newBuilding = Instantiate(SelectedBuilding.constructionManagerPrefab, position, Quaternion.identity);
        var damagableScript = newBuilding.GetComponent<Damagable>();
        damagableScript.playerId = PlayerController.Instance.playerId;
        var unitScript = newBuilding.GetComponent<Unit>();
        unitScript.playerId = PlayerController.Instance.playerId;
    }

    private void PlaceBuilding() {
        if (Input.GetMouseButtonDown(0) && IsValidPosition()) {
            PlaceBuilding(previewPrefab.transform.position);
            CancelBuilding();
        }
    }

    private bool IsValidPosition() {
        return placeableBuilding != null && placeableBuilding.colliders.Count == 0;
    }

    public void CancelBuilding() {
        if (previewPrefab != null) Destroy(previewPrefab);
        SelectedBuilding = null;
        UIBuildingManager.Instance.SetSelectedBuilding(null);
    }

    private void CheckSelectedBuilding() {
        var selectedBuilding = UIBuildingManager.Instance.GetSelectedBuilding();

        if (selectedBuilding != null && selectedBuilding != SelectedBuilding) {
            SetSelectedBuilding(selectedBuilding);
        }

        if (SelectedBuilding != null && previewPrefab == null) {
            var mousePosition = GetMouseWorldPosition();

            if (mousePosition != null) {
                previewPrefab = Instantiate(SelectedBuilding.validPrefab);
                previewPrefab.transform.position = (Vector3)mousePosition;
                placeableBuilding = previewPrefab.GetComponent<PlaceableBuilding>();
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) {
            CancelBuilding();
        }

        CheckSelectedBuilding();
        PlaceBuilding();
        BuildingPreview();
    }
}
