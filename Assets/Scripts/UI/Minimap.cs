using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MiniMapRectangle : NetworkBehaviour
{
    public VisualElement minimapImage;
    public Camera minimap;
    public CameraSystem cameraSystem;
    public Collider terrainCollider;
    public float lineWidth = 0.001f;
    public UIDocument UIDocument;
    private bool isPressed = false;

    public RenderTexture minimapRenderTexture;

    void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();

        var root = UIDocument.rootVisualElement;
        minimapImage = root.Q<VisualElement>("Minimap");
        // add click listener to the minimap
        minimapImage.RegisterCallback<PointerDownEvent>(HandleMinimapClick);
        RenderPipelineManager.endCameraRendering += HandlePostRender;
    }

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        cameraSystem = FindAnyObjectByType<CameraSystem>();
        terrainCollider = Terrain.activeTerrain.GetComponent<Collider>();
        minimap = GameObject.Find("MinimapCamera").GetComponent<Camera>();
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
        // Create a temporary texture to draw on
        Texture2D tempTexture = new Texture2D(minimapRenderTexture.width, minimapRenderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = minimapRenderTexture;
        tempTexture.ReadPixels(new Rect(0, 0, minimapRenderTexture.width, minimapRenderTexture.height), 0, 0);
        tempTexture.Apply();

        // Calculate the camera's view corners in world space
        Vector3[] corners = GetCameraViewCorners(Camera.main);

        // Convert world space corners to minimap texture space
        Vector2[] textureCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            textureCorners[i] = WorldToMinimapTextureSpace(corners[i]);
        }

        // Draw the trapezoid
        DrawLine(tempTexture, textureCorners[0], textureCorners[1], Color.red);
        DrawLine(tempTexture, textureCorners[1], textureCorners[2], Color.red);
        DrawLine(tempTexture, textureCorners[2], textureCorners[3], Color.red);
        DrawLine(tempTexture, textureCorners[3], textureCorners[0], Color.red);

        // Apply changes and copy back to the render texture
        tempTexture.Apply();
        Graphics.Blit(tempTexture, minimapRenderTexture);
        RenderTexture.active = null;

        // Clean up
        DestroyImmediate(tempTexture);
    }

    private Vector3[] GetCameraViewCorners(Camera camera)
    {
        // Get the camera frustum corners
        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        // Convert frustum corners to world space
        for (int i = 0; i < 4; i++)
        {
            frustumCorners[i] = camera.transform.TransformVector(frustumCorners[i]);
            frustumCorners[i] += camera.transform.position;
        }

        // Log the results
        for (int i = 0; i < 4; i++)
        {
            // shoot a ray from the camera to the corner to hit terrain
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, frustumCorners[i] - camera.transform.position, out hit, 2000, LayerMask.GetMask("FlatMap")))
            {
                var hitPoint = new Vector3(hit.point.x, 0, hit.point.z);
                frustumCorners[i] = hitPoint;
            }
        }

        return frustumCorners;
    }

    private Vector2 WorldToMinimapTextureSpace(Vector3 worldPosition)
    {
        var terrainWidth = Terrain.activeTerrain.terrainData.size.x;
        var terrainHeight = Terrain.activeTerrain.terrainData.size.z;
        var textureWidth = minimapRenderTexture.width;
        var textureHeight = minimapRenderTexture.height;

        var x = worldPosition.x / terrainWidth * textureWidth;
        var y = worldPosition.z / terrainHeight * textureHeight;

        return new Vector2(x, y);
    }

    private void DrawLine(Texture2D texture, Vector2 start, Vector2 end, Color color)
    {
        int x0 = Mathf.RoundToInt(Math.Clamp(start.x, 0, minimapRenderTexture.width));
        int y0 = Mathf.RoundToInt(Math.Clamp(start.y, 0, minimapRenderTexture.height - 1));
        int x1 = Mathf.RoundToInt(Math.Clamp(end.x, 0, minimapRenderTexture.width));
        int y1 = Mathf.RoundToInt(Math.Clamp(end.y, 0, minimapRenderTexture.height - 1));

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            texture.SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
}