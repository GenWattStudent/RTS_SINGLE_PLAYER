using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SelectionManager : NetworkBehaviour
{
    private bool isDragging = false;
    private Vector3 mouseStartPosition;
    private Vector3 mouseThreshold = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private RectTransform selectionBox;
    // select event
    public delegate void SelectAction();
    public static event SelectAction OnSelect;

    public static void DeselectAll(PlayerData playerData)
    {
        foreach (Selectable selectable in playerData.selectableObjects)
        {
            selectable.Deselect();
        }

        OnSelect?.Invoke();
        playerData.selectableObjects.Clear();
    }

    private void Start()
    {
        if (!IsOwner) enabled = false;
    }

    private bool IsEnemy(Selectable selectable, ulong clientId)
    {
        if (selectable == null) return true;
        var unitScript = selectable.GetComponent<Unit>();
        if (unitScript == null) return true;

        Debug.Log($"unitScript.OwnerClientId: {unitScript.OwnerClientId}, PlayerController.Instance.OwnerClientId: {clientId}");
        return unitScript.OwnerClientId != clientId;
    }

    public static bool IsCanAttack(ulong clientId)
    {
        var playerData = MultiplayerController.Instance.Get(clientId);
        if (playerData.selectableObjects.Count == 0) return false;
        foreach (Selectable selectable in playerData.selectableObjects)
        {
            var unitScript = selectable.GetComponent<Unit>();
            if (unitScript == null || unitScript.unitSo == null || unitScript.unitSo.type == UnitSo.UnitType.Worker) continue;
            return true;
        }

        return false;
    }

    public static List<Selectable> GetWorkers(ulong clientId)
    {
        var playerData = MultiplayerController.Instance.Get(clientId);
        var workers = new List<Selectable>();

        foreach (Selectable selectable in playerData.selectableObjects)
        {
            var unitScript = selectable.GetComponent<Unit>();
            if (unitScript != null && unitScript.unitSo != null && unitScript.unitSo.type == UnitSo.UnitType.Worker)
            {
                workers.Add(selectable);
            }
        }

        return workers;
    }

    public static List<Selectable> GetHealers(ulong clientId)
    {
        var playerData = MultiplayerController.Instance.Get(clientId);
        var healers = new List<Selectable>();

        foreach (Selectable selectable in playerData.selectableObjects)
        {
            var unitScript = selectable.GetComponent<Unit>();
            var healerScript = selectable.GetComponent<Healer>();

            if (unitScript != null && unitScript.unitSo != null && healerScript != null)
            {
                healers.Add(selectable);
            }
        }

        return healers;
    }

    public static bool IsBuilding(Selectable selectable)
    {
        if (selectable == null) return false;
        var buildingScript = selectable.GetComponent<Building>();
        if (buildingScript == null) return false;
        return true;
    }

    public static void Select(Selectable selectable, PlayerData playerData)
    {
        selectable.Select();
        playerData.selectableObjects.Add(selectable);
        OnSelect?.Invoke();
    }

    public static void Deselect(Selectable selectable, PlayerData playerData)
    {
        selectable.Deselect();
        playerData.selectableObjects.Remove(selectable);
        OnSelect?.Invoke();
    }

    public static void SelectBuilding(Selectable selectable, PlayerData playerData)
    {
        DeselectAll(playerData);
        var buildingScript = selectable.GetComponent<Building>();
        var tankBuildingScript = selectable.GetComponent<TankBuilding>();
        var constructionScript = selectable.GetComponent<Construction>();

        if (constructionScript != null)
        {
            selectable.Select();
            playerData.selectableObjects.Add(selectable);
            return;
        }

        if (tankBuildingScript != null) UIUnitManager.Instance.CreateUnitTabs(buildingScript.buildingSo, tankBuildingScript, tankBuildingScript.gameObject);
        selectable.Select();
        playerData.selectableObjects.Add(selectable);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnClickHandlerServerRpc(Vector2 postion, ServerRpcParams serverRpcParams = default)
    {
        // Get all the objects that are under the mouse position its 3D project!!
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(postion), 100f);
        var playerData = MultiplayerController.Instance.Get(serverRpcParams.Receive.SenderClientId);
        bool isSelectableClicked = false;
        // Loop through all the objects
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("ForceField")) continue;
            // Check if the object is selectable
            Selectable selectable = hit.collider.GetComponent<Selectable>();

            if (IsBuilding(selectable) && !IsEnemy(selectable, serverRpcParams.Receive.SenderClientId))
            {
                SelectBuilding(selectable, playerData);
                isSelectableClicked = true;
                break;
            }

            if (!IsEnemy(selectable, serverRpcParams.Receive.SenderClientId))
            {
                // Check if the object is already selected
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (selectable.isSelected)
                    {
                        Deselect(selectable, playerData);
                    }
                    else
                    {
                        Select(selectable, playerData);
                    }

                    isSelectableClicked = true;
                    break;
                }

                DeselectAll(playerData);
                Select(selectable, playerData);
                isSelectableClicked = true;
                break;
            }
        }

        if (!isSelectableClicked)
        {
            DeselectAll(playerData);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectObjectsInRectangleServerRpc(Vector2 anchoredPosition, Vector2 sizeDelta, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var playerData = MultiplayerController.Instance.Get(clientId);
        DeselectAll(playerData);

        Vector2 min = anchoredPosition - (sizeDelta / 2);
        Vector2 max = anchoredPosition + (sizeDelta / 2);

        foreach (Unit unit in playerData.units)
        {
            var selectable = unit.GetComponent<Selectable>();
            if (IsEnemy(selectable, clientId) || IsBuilding(selectable)) continue;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectable.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                selectable.Select();
                playerData.selectableObjects.Add(selectable);
            }
        }

        OnSelect?.Invoke();
    }

    private void UpdateSelectionBox(Vector2 currentMousePosition)
    {
        if (!selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(true);
        }

        float width = currentMousePosition.x - mouseStartPosition.x;
        float height = currentMousePosition.y - mouseStartPosition.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = mouseStartPosition + new Vector3(width / 2, height / 2);
    }

    // Select unit if clicked on it, deselect when nothing is clicked on
    private void Update()
    {
        if (UIHelper.Instance.IsPointerOverUIElement())
        {
            if (isDragging)
            {
                SelectObjectsInRectangleServerRpc(selectionBox.anchoredPosition, selectionBox.sizeDelta);
                selectionBox.gameObject.SetActive(false);
                selectionBox.sizeDelta = Vector2.zero;
                isDragging = false;
                return;
            }

            return;
        };

        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPosition = Input.mousePosition;
        }

        // drag selection
        if (Input.GetMouseButton(0))
        {
            // if compare to mouseStartPosition is less than mouseThreshold, return
            if (Vector3.Distance(mouseStartPosition, Input.mousePosition) < mouseThreshold.magnitude) return;

            isDragging = true;
            UpdateSelectionBox(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging) SelectObjectsInRectangleServerRpc(selectionBox.anchoredPosition, selectionBox.sizeDelta);
            else OnClickHandlerServerRpc(Input.mousePosition);
            isDragging = false;
        }
    }
}
