using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MiniMapRectangle : MonoBehaviour
{
    public Material cameraBoxMaterial;
    public RawImage minimapImage;
    public Camera mainCamera;
    public Camera minimap;
    public CameraSystem cameraSystem;
    public Collider terrainCollider;
    public float lineWidth = 0.001f;
    public Vector3 topLeftPosition, topRightPosition, bottomLeftPosition, bottomRightPosition;

    void OnEnable()
    {
        // add click listener to the minimap
        minimapImage.GetComponent<Button>().onClick.AddListener(MoveCameraToMousePosition);
        // add post render listener to the minimap camera 

        // add post render listener to the minimap camera
        // RenderPipelineManager.endCameraRendering += OnPostRenderr;
    }

    void MoveCameraToMousePosition()
    {
        // calculate mouse position - minimap postion
        Vector2 mousePosition = Input.mousePosition;
        Vector2 minimapPosition = minimapImage.transform.position;
        Vector2 mousePositionOnMinimap = mousePosition - minimapPosition;
        // Take terrain widht and height and calculate camera postion
        float terrainWidth = Terrain.activeTerrain.terrainData.size.x;
        float terrainHeight = Terrain.activeTerrain.terrainData.size.z;
        float minimapPercentageX = mousePositionOnMinimap.x / (minimapImage.rectTransform.rect.width * 0.6f);
        float minimapPercentageY = mousePositionOnMinimap.y / (minimapImage.rectTransform.rect.height * 0.6f);
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