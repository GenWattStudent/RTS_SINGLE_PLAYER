using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Minimap : NetworkToolkitHelper
{
    public float lineWidth = 1f;

    private VisualElement minimapImage;
    private CameraSystem cameraSystem;
    private bool isPressed = false;
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private VisualElement bigMap;

    protected override void OnEnable()
    {
        base.OnEnable();
        minimapImage = GetVisualElement("Minimap");
        bigMap = GetVisualElement("BigMap");
        // add click listener to the minimap
        minimapImage.RegisterCallback<PointerDownEvent>(HandleMinimapClick);
    }

    private void OnDisable()
    {
        minimapImage.UnregisterCallback<PointerDownEvent>(HandleMinimapClick);
        RenderPipelineManager.endCameraRendering -= HandlePostRender;
    }

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        cameraSystem = FindAnyObjectByType<CameraSystem>();
        mainCamera = Camera.main;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        // add layer to line renderer
        lineRenderer.gameObject.layer = LayerMask.NameToLayer("Trapezoid");
        // set line renderer to be in front of minimap
        lineRenderer.sortingOrder = 1;

        RenderPipelineManager.endCameraRendering += HandlePostRender;
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (bigMap.style.display == DisplayStyle.Flex)
            {
                bigMap.style.display = DisplayStyle.None;
            }
            else
            {
                bigMap.style.display = DisplayStyle.Flex;
            }
        }

        if (isPressed)
        {
            MoveCameraToMousePosition(Input.mousePosition);
        }
    }

    private Vector3 GetMinimapPositionToWorld(Vector2 position)
    {
        float terrainWidth = Terrain.activeTerrain.terrainData.size.x;
        float terrainHeight = Terrain.activeTerrain.terrainData.size.z;
        var xPos = position.x - minimapImage.worldBound.position.x;

        var minimapPercentageX = xPos / minimapImage.resolvedStyle.width;
        var minimapPercentageY = position.y / minimapImage.resolvedStyle.height;
        float cameraX = terrainWidth * minimapPercentageX;
        float cameraZ = terrainHeight * minimapPercentageY;
        // Move camera to calculated position
        return new Vector3(cameraX, cameraSystem.transform.position.y, cameraZ);
    }

    private void MoveCameraToMousePosition(Vector2 position)
    {
        // Move camera to calculated position
        cameraSystem.transform.position = GetMinimapPositionToWorld(position);
    }

    private void HandlePostRender(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "MinimapCamera")
        {
            DrawCameraViewTrapezoid();
        }
    }

    private void DrawCameraViewTrapezoid()
    {
        Vector3[] corners = GetCameraViewCorners(mainCamera);

        for (int i = 0; i < 4; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }

        lineRenderer.SetPosition(4, corners[0]);
    }

    private Vector3[] GetCameraViewCorners(Camera camera)
    {
        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        for (int i = 0; i < 4; i++)
        {
            frustumCorners[i] = camera.transform.TransformVector(frustumCorners[i]);
            frustumCorners[i] += camera.transform.position;
        }

        for (int i = 0; i < 4; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, frustumCorners[i] - camera.transform.position, out hit, 2000, LayerMask.GetMask("FlatMap")))
            {
                var hitPoint = new Vector3(hit.point.x, 0, hit.point.z);
                frustumCorners[i] = hitPoint;
            }
        }

        return frustumCorners;
    }
}