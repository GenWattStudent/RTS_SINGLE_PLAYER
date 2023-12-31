using UnityEngine;

public class BuildingSystem : Singleton<BuildingSystem>
{
    [SerializeField] private LayerMask terrainLayer;
    public BuildingSo SelectedBuilding { get; private set; }
    private PlaceableBuilding placeableBuilding;
    private GameObject previewPrefab;
    private bool wasValid = false;
    private GameObject[] heightsPoint;
    public float diffranceBetweenMaxAndMinHeight = 1f;
    private Popup popup;

    private bool IsFlatTerrain() {
        if (heightsPoint == null) return false;

        // make raycast fromm the height points to the ground then chek diff between  min and max height
        float maxHeight = 0;
        float minHeight = 0;

        foreach (var point in heightsPoint) {
            var rayPosition = new Vector3(point.transform.position.x, 100f, point.transform.position.z);
            Ray ray = new Ray(rayPosition, Vector3.down);
            // if ray hit notthing then return false
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, terrainLayer)) {
                if (hit.point.y > maxHeight) maxHeight = hit.point.y;
                if (hit.point.y < minHeight) minHeight = hit.point.y;
            } else {
                return false;
            }
        }

        return Mathf.Abs(maxHeight - minHeight) <= diffranceBetweenMaxAndMinHeight;
    }
 
    public void SetSelectedBuilding(BuildingSo building) {
        SelectedBuilding = building;
        if (previewPrefab) Destroy(previewPrefab);
    }

    private Vector3? GetMouseWorldPosition() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, terrainLayer)) {
            return hit.point;
        } else {
            return null;
        }
    }

    private void GetHeightPoints() {
        if (previewPrefab == null) return;
        var heightPoints = previewPrefab.transform.Find("HeightPoints");

        if (heightPoints != null) {
            heightsPoint = new GameObject[heightPoints.childCount];

            for (int i = 0; i < heightPoints.childCount; i++) {
                heightsPoint[i] = heightPoints.GetChild(i).gameObject;
            }
        }
    }

    private void SetPopup() {
        if (popup == null) {
            var message = $"{SelectedBuilding.name} ({PlayerController.Instance.GetBuildingCountOfType(SelectedBuilding)}/{SelectedBuilding.maxBuildingCount})";
            popup = PopupManager.Instance.ShowPopup(message, Vector3.zero, -1, true, new Vector2(25, 0));
        }
    }

    private void BuildingPreview() {
        if (SelectedBuilding is null) return;

        var mousePosition = GetMouseWorldPosition();
        bool isValid = IsValidPosition();

        SetPopup();
        if (isValid != wasValid && mousePosition != null) {
            if (previewPrefab != null) Destroy(previewPrefab);
            if (isValid) {
                previewPrefab = Instantiate(SelectedBuilding.validPrefab);
                GetHeightPoints();
            } else {
                previewPrefab = Instantiate(SelectedBuilding.invalidPrefab);
                GetHeightPoints();
            }

            placeableBuilding = previewPrefab.GetComponent<PlaceableBuilding>();
            wasValid = isValid;
        }

        if (mousePosition != null && previewPrefab != null) previewPrefab.transform.position = (Vector3)mousePosition;
    }

    private void PlaceBuilding(Vector3 position) {
        if (SelectedBuilding == null || !UIStorage.Instance.HasEnoughResource(SelectedBuilding.costResource, SelectedBuilding.cost)) return;
        UIStorage.Instance.DecreaseResource(SelectedBuilding.costResource, SelectedBuilding.cost);

        var newBuilding = Instantiate(SelectedBuilding.constructionManagerPrefab, position, Quaternion.identity);
        var damagableScript = newBuilding.GetComponent<Damagable>();

        damagableScript.health = 1;
        damagableScript.playerId = PlayerController.Instance.playerId;

        var unitScript = newBuilding.GetComponent<Unit>();
        unitScript.playerId = PlayerController.Instance.playerId;
        PlayerController.Instance.AddBuilding(newBuilding.GetComponent<Building>());
    }

    private void PlaceBuilding() {
        if (Input.GetMouseButtonDown(0) && IsValidPosition()) {
            PlaceBuilding(previewPrefab.transform.position);
            CancelBuilding();
        }
    }

    private bool IsValidPosition() {
        return placeableBuilding != null && 
        placeableBuilding.colliders.Count == 0 && 
        !PlayerController.Instance.IsMaxBuildingOfType(SelectedBuilding) && 
        IsFlatTerrain();
    }

    public void CancelBuilding() {
        if (previewPrefab != null) Destroy(previewPrefab);
        SelectedBuilding = null;
        UIBuildingManager.Instance.SetSelectedBuilding(null);
        PopupManager.Instance.DestroyPopup(popup);
        popup = null;
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
                GetHeightPoints();
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
