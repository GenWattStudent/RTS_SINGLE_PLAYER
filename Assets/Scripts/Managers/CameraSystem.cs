using UnityEngine;
using Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private bool isEdgdeScrolling = true;
    [SerializeField] private bool isCameraZooming = true;
    [SerializeField] private bool isCameraRotating = true;
    [SerializeField] private float scrollSize = 25;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private float targetFieldOfView = 50f;
    [SerializeField] private float maxZoom = 50f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float cameraRotationSpeed = 40f;
    [SerializeField] private float cameraZoomSpeed = 5f;
    [SerializeField] private float cameraMovementSpeed = 40f;
    [SerializeField] private float marginBottom = -5f;
    [SerializeField] private float marginTop = 5f;
    [SerializeField] private float marginLeft = -5f;
    [SerializeField] private float marginRight = 5f;
    private Terrain terrain;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        terrain = Terrain.activeTerrain;
    }

    private void Update()
    {
        HandleCameraMovement();

        if (isEdgdeScrolling)
        {
            HandleEdgeScrolling();
        }

        if (isCameraZooming)
        {
            HandleCameraZoom();
        }

        if (isCameraRotating)
        {
            HandleCameraRotation();
        }
    }

    private void HandleEdgeScrolling()
    {
        Vector3 inputDirection = new Vector3(0, 0, 0);

        if (Input.mousePosition.x < scrollSize)
        {
            inputDirection.x = -1f;
        }

        if (Input.mousePosition.x > Screen.width - scrollSize)
        {
            inputDirection.x = +1f;
        }

        if (Input.mousePosition.y < scrollSize)
        {
            inputDirection.z = -1f;
        }

        if (Input.mousePosition.y > Screen.height - scrollSize)
        {
            inputDirection.z = +1f;
        }

        UpdateCameraPosition(inputDirection);
    }

    private void UpdateCameraPosition(Vector3 inputDirection)
    {
        Vector3 moveDirection = transform.forward * inputDirection.z + transform.right * inputDirection.x;
        Vector3 newPosition = transform.position + moveDirection * cameraMovementSpeed * Time.deltaTime;

        if (terrain != null)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, marginLeft, terrain.terrainData.size.x + marginRight);
            newPosition.z = Mathf.Clamp(newPosition.z, marginBottom, terrain.terrainData.size.z + marginTop);
        }

        transform.position = newPosition;
    }

    private void HandleCameraMovement()
    {
        Vector3 inputDirection = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            inputDirection.z = +1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            inputDirection.z = -1f;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            inputDirection.x = -1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            inputDirection.x = +1f;
        }

        UpdateCameraPosition(inputDirection);
    }

    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= 5f;
        }

        if (Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += 5f;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, minZoom, maxZoom);
        cinemachineVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.FieldOfView, targetFieldOfView, Time.deltaTime * cameraZoomSpeed);
    }

    private void HandleCameraRotation()
    {
        float rotateDirection = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotateDirection = +1f;
        }

        if (Input.GetKey(KeyCode.E))
        {
            rotateDirection = -1f;
        }

        transform.eulerAngles += new Vector3(0, rotateDirection * cameraRotationSpeed * Time.deltaTime, 0);
    }

    public void SetCameraPosition(Vector3 position)
    {
        transform.position = position;
    }
}
