using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingManager : NetworkBehaviour
{
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    [SerializeField] private List<BuildingSo> networkConstructionsPrefabs;
    public BuildingSo SelectedBuilding { get; private set; }
    public int heightRaysCount = 15;
    public float differenceBetweenMaxAndMinHeight = 1f;

    private GameObject previewPrefab;
    private PlayerController playerController;
    private RTSObjectsManager RTSObjectsManager;
    private UIBuildingManager uIBuildingManager;
    private UIStorage uIStorage;
    private InfoBox infoBox;
    private BuildingValidator buildingValidator;
    private float currentRotation = 0f; // Track the current rotation of the building

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        RTSObjectsManager = GetComponent<RTSObjectsManager>();
        uIBuildingManager = GetComponentInChildren<UIBuildingManager>();
        uIStorage = GetComponentInChildren<UIStorage>();

        buildingValidator = new BuildingValidator(heightRaysCount, terrainLayer, differenceBetweenMaxAndMinHeight);
    }

    private void Start()
    {
        infoBox = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<InfoBox>();
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

    private void RotateBuildingPreview()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            currentRotation += Input.mouseScrollDelta.y > 0 ? 90f : -90f;
            currentRotation = Mathf.Round(currentRotation / 90f) * 90f; // Ensure rotation is a multiple of 90
            if (previewPrefab != null)
            {
                previewPrefab.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
            }
        }
    }

    private void SetPopup()
    {
        var message = $"{SelectedBuilding.name} ({RTSObjectsManager.GetBuildingCountOfType(SelectedBuilding)}/{SelectedBuilding.maxBuildingCount})";
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
        buildingValidator.GenerateHeightPoints(previewPrefab);
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

        if (mousePosition != null && previewPrefab != null)
        {
            previewPrefab.transform.position = (Vector3)mousePosition;
            RotateBuildingPreview();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceBuildingServerRpc(Vector3 position, ushort buildingIndex, ulong clientId)
    {
        var buildingSo = networkConstructionsPrefabs[buildingIndex];

        if (!uIStorage.HasEnoughResource(buildingSo.costResource, buildingSo.cost)) return;
        uIStorage.DecreaseResource(buildingSo.costResource, buildingSo.cost);

        var newBuilding = Instantiate(buildingSo.ConstructionManagerPrefab, position, Quaternion.Euler(0, currentRotation, 0));
        var no = newBuilding.GetComponent<NetworkObject>();
        var stats = newBuilding.GetComponent<Stats>();
        var damagable = newBuilding.GetComponent<Damagable>();
        var construction = newBuilding.GetComponent<Construction>();

        construction.construction = buildingSo;
        no.SpawnWithOwnership(clientId);
        stats.SetStat(StatType.Health, 1);
        damagable.teamType.Value = playerController.teamType.Value;
        RTSObjectsManager.AddBuildingServerRpc(no);
    }

    private void PlaceBuilding()
    {
        if (Input.GetMouseButtonDown(0) && SelectedBuilding != null && !UIHelper.Instance.IsPointerOverUIElement())
        {
            if (!IsValidPosition())
            {
                infoBox.AddError("You cant place building here!");
                return;
            };

            var buildingIndex = (ushort)networkConstructionsPrefabs.IndexOf(SelectedBuilding);
            if (previewPrefab != null) PlaceBuildingServerRpc(previewPrefab.transform.position, buildingIndex, OwnerClientId);
            CancelBuilding();
        }
    }

    private bool IsValidPosition()
    {
        return !RTSObjectsManager.IsMaxBuildingOfType(SelectedBuilding) && buildingValidator.IsValid();
    }

    public void CancelBuilding()
    {
        if (previewPrefab != null) Destroy(previewPrefab);
        SelectedBuilding = null;
        uIBuildingManager.SetSelectedBuilding(null);
        MousePopup.Instance.Hide();
    }

    private void CheckSelectedBuilding()
    {
        var selectedBuilding = uIBuildingManager.GetSelectedBuilding();

        if (selectedBuilding != null && selectedBuilding != SelectedBuilding)
        {
            SetSelectedBuilding(selectedBuilding);
        }

        if (SelectedBuilding != null && previewPrefab == null)
        {
            var mousePosition = GetMouseWorldPosition();

            if (mousePosition != null)
            {
                previewPrefab = Instantiate(SelectedBuilding.PreviewPrefab);
                previewPrefab.transform.position = (Vector3)mousePosition;
                buildingValidator.GenerateHeightPoints(previewPrefab);
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
