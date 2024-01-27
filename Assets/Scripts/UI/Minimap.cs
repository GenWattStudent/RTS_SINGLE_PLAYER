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
    public UIDocument UIDocument;
    private bool isPressed = false;

    void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        var root = UIDocument.rootVisualElement;
        minimapImage = root.Q<VisualElement>("Minimap");
        // add click listener to the minimap
        minimapImage.RegisterCallback<PointerDownEvent>(HandleMinimapClick);
        RenderPipelineManager.endCameraRendering += OnPostRenderr;
    }

    private void HandleMinimapClick(PointerDownEvent ev)
    {
        isPressed = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isPressed = false;
        }
        
        if (isPressed)
        {
            MoveCameraToMousePosition(Input.mousePosition);
        }
    }

    void MoveCameraToMousePosition(Vector2 position)
    {
        // calculate mouse position - minimap postion
        float terrainWidth = Terrain.activeTerrain.terrainData.size.x;
        float terrainHeight = Terrain.activeTerrain.terrainData.size.z;
        var xPos = position.x - minimapImage.worldBound.position.x;

        var minimapPercentageX = xPos / minimapImage.resolvedStyle.width;
        var minimapPercentageY = position.y / minimapImage.resolvedStyle.height;
        float cameraX = terrainWidth * minimapPercentageX;
        float cameraZ = terrainHeight * minimapPercentageY;
        // Move camera to calculated position
        cameraSystem.transform.position = new Vector3(cameraX, cameraSystem.transform.position.y, cameraZ);
    }

    void OnPostRenderr(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "MinimapCamera") {

        }
    }
}