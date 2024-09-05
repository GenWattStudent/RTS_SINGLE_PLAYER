using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MiniMapRectangle : NetworkBehaviour
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

    public RenderTexture minimapRenderTexture;
    private Texture2D minimapTexture;

    void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        mainCamera = Camera.main;
        cameraSystem = FindAnyObjectByType<CameraSystem>();
        terrainCollider = Terrain.activeTerrain.GetComponent<Collider>();
        minimap = GameObject.Find("MinimapCamera").GetComponent<Camera>();
        var root = UIDocument.rootVisualElement;
        minimapImage = root.Q<VisualElement>("Minimap");
        // add click listener to the minimap
        minimapImage.RegisterCallback<PointerDownEvent>(HandleMinimapClick);
        RenderPipelineManager.endCameraRendering += OnPostRenderr;

        minimapRenderTexture = new RenderTexture(256, 256, 24);
        minimap.targetTexture = minimapRenderTexture;
        minimapTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
    }

    void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
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

    private void OnPostRenderr(ScriptableRenderContext context, Camera camera)
    {
        if (camera.name == "MinimapCamera")
        {
            // DrawCameraViewTrapezoid();
        }
    }

    void DrawCameraViewTrapezoid()
    {
        RenderTexture.active = minimapRenderTexture;
        minimapTexture.ReadPixels(new Rect(0, 0, minimapRenderTexture.width, minimapRenderTexture.height), 0, 0);
        minimapTexture.Apply();

        Vector3[] frustrumCorners = new Vector3[4];
        mainCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), mainCamera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustrumCorners);

        for (int i = 0; i < 4; i++)
        {
            frustrumCorners[i] = mainCamera.transform.TransformPoint(frustrumCorners[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            Ray ray = new Ray(frustrumCorners[i] + mainCamera.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            {
                frustrumCorners[i] = hit.point;
            }
        }

        Vector2[] minimapCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            minimapCorners[i] = WorldToMinimapPoint(frustrumCorners[i]);
            Debug.Log($"Minimap corner {i}: {minimapCorners[i]}");
        }

        for (int i = 0; i < 4; i++)
        {
            DrawLine(minimapTexture, minimapCorners[i], minimapCorners[(i + 1) % 4], Color.red);
        }

        minimapImage.style.backgroundImage = Background.FromTexture2D(minimapTexture);
    }

    Vector2 WorldToMinimapPoint(Vector3 worldPoint)
    {
        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
        float x = (worldPoint.x / terrainSize.x) * minimapTexture.width;
        float y = (worldPoint.z / terrainSize.z) * minimapTexture.height;
        return new Vector2(x, y);
    }

    void DrawLine(Texture2D texture, Vector2 start, Vector2 end, Color color)
    {
        int x0 = Mathf.RoundToInt(start.x);
        int y0 = Mathf.RoundToInt(start.y);
        int x1 = Mathf.RoundToInt(end.x);
        int y1 = Mathf.RoundToInt(end.y);

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < texture.width && y0 >= 0 && y0 < texture.height)
            {
                texture.SetPixel(x0, y0, color);
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}