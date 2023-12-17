using System;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapRectangle : MonoBehaviour
{
    public RawImage minimapImage;
    public Camera mainCamera;
    public CameraSystem cameraSystem;
    public RectTransform rectangleTransform;

    void OnEnable()
    {
        // add click listener to the minimap
        minimapImage.GetComponent<Button>().onClick.AddListener(MoveCameraToMousePosition);
    }

    void OnDisable()
    {
        // remove click listener from the minimap
        minimapImage.GetComponent<Button>().onClick.RemoveListener(MoveCameraToMousePosition);
    }

    void Start()
    {

    }

    void Update()
    {
        // UpdateRectangleOnMinimap();
    }

    private void UpdateRectangleOnMinimap()
    {
        // calculate percent of camera postion on the terrain
        float terrainWidth = Terrain.activeTerrain.terrainData.size.x;
        float terrainHeight = Terrain.activeTerrain.terrainData.size.z;
        float cameraX = cameraSystem.transform.position.x;
        float cameraZ = cameraSystem.transform.position.z;
        float minimapPercentageX = cameraX / terrainWidth;
        float minimapPercentageY = cameraZ / terrainHeight;
        // calculate rectangle size
        float rectangleWidth = mainCamera.orthographicSize * 2 * mainCamera.aspect * 0.6f;
        float rectangleHeight = mainCamera.orthographicSize * 2 * 0.6f;
        // calculate rectangle position
        float rectangleX = minimapPercentageX * minimapImage.rectTransform.rect.width * 0.6f;
        float rectangleY = minimapPercentageY * minimapImage.rectTransform.rect.height * 0.6f;
        // update rectangle position and size
        DrawRectangle(new Vector2(rectangleX, rectangleY), new Vector2(rectangleWidth, rectangleHeight));
    }

    void DrawRectangle(Vector2 position, Vector2 size)
    {
        // calculate rectangle points
        Vector2 topLeft = position - size / 2;
        Vector2 topRight = new Vector2(position.x + size.x / 2, position.y - size.y / 2);
        Vector2 bottomRight = position + size / 2;
        Vector2 bottomLeft = new Vector2(position.x - size.x / 2, position.y + size.y / 2);
        // draw rectangle

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
}