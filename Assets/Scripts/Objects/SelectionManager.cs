using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SelectionManager : NetworkBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    [HideInInspector] public List<Selectable> selectedObjects;

    private bool isDragging = false;
    private Vector3 mouseStartPosition;
    private Vector3 mouseThreshold = new Vector3(0.1f, 0.1f, 0.1f);
    private PlayerController playerController;
    private UIUnitManager UIUnitManager;
    private RTSObjectsManager RTSObjectsManager;

    // select event
    public static event Action<List<Selectable>> OnSelect;

    public void DeselectAll()
    {
        foreach (Selectable selectable in selectedObjects)
        {
            selectable.Deselect();
        }

        selectedObjects.Clear();
        OnSelect?.Invoke(selectedObjects);
    }

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        playerController = GetComponent<PlayerController>();
        UIUnitManager = GetComponentInChildren<UIUnitManager>();
        RTSObjectsManager = GetComponent<RTSObjectsManager>();
        selectedObjects = new();
    }

    private bool IsEnemy(Selectable selectable)
    {
        if (selectable == null) return true;
        var unitScript = selectable.GetComponent<Unit>();
        if (unitScript == null) return true;

        return unitScript.OwnerClientId != playerController.OwnerClientId;
    }

    public bool IsCanAttack()
    {
        if (selectedObjects.Count == 0) return false;
        foreach (Selectable selectable in selectedObjects)
        {
            var unitScript = selectable.GetComponent<Unit>();
            if (unitScript == null || unitScript.unitSo == null || unitScript.unitSo.type == UnitSo.UnitType.Worker) continue;
            return true;
        }

        return false;
    }

    public List<Selectable> GetWorkers()
    {
        var workers = new List<Selectable>();

        foreach (Selectable selectable in selectedObjects)
        {
            var unitScript = selectable.GetComponent<Unit>();
            if (unitScript != null && unitScript.unitSo != null && unitScript.unitSo.type == UnitSo.UnitType.Worker)
            {
                workers.Add(selectable);
            }
        }

        return workers;
    }

    public List<Selectable> GetHealers()
    {
        var healers = new List<Selectable>();

        foreach (Selectable selectable in selectedObjects)
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

    public bool IsBuilding(Selectable selectable)
    {
        if (selectable == null) return false;
        var buildingScript = selectable.GetComponent<Building>();
        if (buildingScript == null) return false;
        return true;
    }

    public void Select(Selectable selectable)
    {
        selectable.Select();
        selectedObjects.Add(selectable);
        OnSelect?.Invoke(selectedObjects);
    }

    public void Deselect(Selectable selectable)
    {
        selectable.Deselect();
        selectedObjects.Remove(selectable);
        OnSelect?.Invoke(selectedObjects);
    }

    public void SelectBuilding(Selectable selectable)
    {
        DeselectAll();
        var buildingScript = selectable.GetComponent<Building>();
        var Spawner = selectable.GetComponent<Spawner>();
        var constructionScript = selectable.GetComponent<Construction>();

        if (constructionScript != null)
        {
            selectable.Select();
            selectedObjects.Add(selectable);
            OnSelect?.Invoke(selectedObjects);
            return;
        }

        if (Spawner != null) UIUnitManager.CreateUnitTabs(buildingScript.buildingSo, Spawner);
        selectable.Select();
        selectedObjects.Add(selectable);
        OnSelect?.Invoke(selectedObjects);
    }

    private void OnClickHandler()
    {
        // Get all the objects that are under the mouse position its 3D project!!
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100f);
        bool isSelectableClicked = false;
        // Loop through all the objects
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("ForceField")) continue;
            // Check if the object is selectable
            Selectable selectable = hit.collider.GetComponent<Selectable>();

            Debug.Log(selectable);

            if (IsBuilding(selectable) && !IsEnemy(selectable))
            {
                SelectBuilding(selectable);
                isSelectableClicked = true;
                break;
            }

            if (!IsEnemy(selectable))
            {
                // Check if the object is already selected
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (selectable.isSelected)
                    {
                        Deselect(selectable);
                    }
                    else
                    {
                        Select(selectable);
                    }

                    isSelectableClicked = true;
                    break;
                }

                DeselectAll();
                Select(selectable);
                isSelectableClicked = true;
                break;
            }
        }

        if (!isSelectableClicked)
        {
            DeselectAll();
        }
    }

    private void SelectObjectsInRectangle()
    {
        DeselectAll();
        selectionBox.gameObject.SetActive(false);

        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        foreach (Unit unit in RTSObjectsManager.LocalPlayerUnits)
        {
            if (unit == null) continue; // Check if the unit is null
            var selectable = unit.GetComponent<Selectable>();
            if (IsEnemy(selectable) || IsBuilding(selectable)) continue;

            Collider collider = unit.GetComponent<Collider>();
            if (collider == null) continue;

            if (IsColliderInSelectionRectangle(collider, min, max))
            {
                selectable.Select();
                selectedObjects.Add(selectable);
            }
        }

        OnSelect?.Invoke(selectedObjects);
        selectionBox.sizeDelta = Vector2.zero;
    }

    private bool IsColliderInSelectionRectangle(Collider collider, Vector2 min, Vector2 max)
    {
        Bounds bounds = collider.bounds;
        Vector3[] corners = new Vector3[8];

        corners[0] = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));
        corners[1] = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
        corners[2] = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
        corners[3] = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
        corners[4] = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
        corners[5] = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
        corners[6] = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
        corners[7] = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));

        foreach (var corner in corners)
        {
            if (corner.x > min.x && corner.x < max.x && corner.y > min.y && corner.y < max.y)
            {
                return true;
            }
        }

        return false;
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
                SelectObjectsInRectangle();
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
            if (isDragging) SelectObjectsInRectangle();
            else OnClickHandler();
            isDragging = false;
        }
    }
}
