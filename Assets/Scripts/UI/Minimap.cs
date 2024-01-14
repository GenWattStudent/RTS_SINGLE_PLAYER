using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MiniMapRectangle : MonoBehaviour
{
    public Material cameraBoxMaterial;
    public VisualElement minimapImage;
    public Camera mainCamera;
    public Camera minimap;
    public CameraSystem cameraSystem;
    public Collider terrainCollider;
    public float lineWidth = 0.001f;
    public Vector3 topLeftPosition, topRightPosition, bottomLeftPosition, bottomRightPosition;
    public UIDocument UIDocument;

    void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        var root = UIDocument.rootVisualElement;
        minimapImage = root.Q<VisualElement>("Minimap");
        // add click listener to the minimap
        minimapImage.RegisterCallback((ClickEvent ev) => {

            Debug.Log($"mousePositionOnMinimap: {Input.mousePosition}");
            MoveCameraToMousePosition(Input.mousePosition);
        });
        // add post render listener to the minimap camera 

        // add post render listener to the minimap camera
        // RenderPipelineManager.endCameraRendering += OnPostRenderr;
    }

    void MoveCameraToMousePosition(Vector2 position)
    {
        // calculate mouse position - minimap postion
        Vector2 mousePositionOnMinimap = position - minimapImage.WorldToLocal(minimapImage.worldBound.position);
        Debug.Log($"mousePositionOnMinimap: {mousePositionOnMinimap} {minimapImage.worldBound.position} {minimapImage.resolvedStyle.width}");
        // Take terrain widht and height and calculate camera postion
        float terrainWidth = Terrain.activeTerrain.terrainData.size.x;
        float terrainHeight = Terrain.activeTerrain.terrainData.size.z;
        float minimapPercentageX = mousePositionOnMinimap.x / (minimapImage.resolvedStyle.width);
        float minimapPercentageY = mousePositionOnMinimap.y / (minimapImage.resolvedStyle.height);
        float cameraX = terrainWidth * minimapPercentageX;
        float cameraZ = terrainHeight * minimapPercentageY;
        // Move camera to calculated position
        cameraSystem.transform.position = new Vector3(cameraX, cameraSystem.transform.position.y, cameraZ);
    }

    private Vector3 GetCameraFrustumPoint(Vector3 position)
    {
        var positionRay = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        Vector3 result = terrainCollider.Raycast(positionRay, out hit, Camera.main.transform.position.y * 2) ? hit.point : new Vector3();

        return result;
    }

    void OnPostRenderr(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "MinimapCamera") 
        {
            // draw trapezoid on minimap

        }
    }
}