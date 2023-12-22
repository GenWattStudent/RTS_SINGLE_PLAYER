using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    [HideInInspector] public List<Selectable> selectedObjects;
    public static SelectionManager Instance;
    [SerializeField] private float dragSelectionDelay = 0.2f;
    private float dragSelectionTimer = 0f;
    private bool isDragging = false;
    private Vector3 mouseStartPosition;
    [SerializeField] private RectTransform selectionBox;
    // select event
    public delegate void SelectAction();
    public event SelectAction OnSelect;

    public void DeselectAll()
    {
        foreach (Selectable selectable in selectedObjects)
        {
            selectable.Deselect();
        }

        OnSelect?.Invoke();
        selectedObjects.Clear();
    }

    private void Awake()
    {
        Instance = this;
        selectedObjects = new List<Selectable>();
    }

    private bool IsEnemy(Selectable selectable) {
        if (selectable == null) return true;
        var unitScript = selectable.GetComponent<Unit>();
        if (unitScript == null) return true;
        return unitScript.playerId != PlayerController.Instance.playerId;
    }

    public List<Selectable> GetWorkers() {
        var workers = new List<Selectable>();

        foreach (Selectable selectable in selectedObjects) {
            var unitScript = selectable.GetComponent<Unit>();
            if (unitScript != null && unitScript.unitSo != null && unitScript.unitSo.type == UnitSo.UnitType.Worker) {
                workers.Add(selectable);
            }
        }

        return workers;
    }

    private void OnClickHandler() {
        // Get all the objects that are under the mouse position its 3D project!!
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100f);
        bool isSelectableClicked = false;
        // Loop through all the objects
        foreach (RaycastHit hit in hits)
        {
            // Check if the object is selectable
            Selectable selectable = hit.collider.GetComponent<Selectable>();
            if (!IsEnemy(selectable))
            {
                // Check if the object is already selected
                if (Input.GetKey(KeyCode.LeftShift)) {
                    if (selectable.isSelected)
                    {
                        selectable.Deselect();
                        selectedObjects.Remove(selectable);
                        OnSelect?.Invoke();
                    }
                    else
                    {
                        selectable.Select();
                        selectedObjects.Add(selectable);
                        OnSelect?.Invoke();
                    }

                    isSelectableClicked = true;
                    break;
                }

                DeselectAll();
                selectable.Select();
                selectedObjects.Add(selectable);
                OnSelect?.Invoke();
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
        selectionBox.gameObject.SetActive(false);
        
        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        foreach (Selectable selectable in FindObjectsOfType<Selectable>())
        {
            if (IsEnemy(selectable)) continue;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectable.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                selectable.Select();
                selectedObjects.Add(selectable);
            }
        }

        OnSelect?.Invoke();
        selectionBox.sizeDelta = Vector2.zero;
    }

    private void UpdateSelectionBox(Vector2 currentMousePosition) {
        if (!selectionBox.gameObject.activeInHierarchy) {
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
        if (!EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPosition = Input.mousePosition;
            if (!isDragging) OnClickHandler();
        }

        // drag selection
        if (Input.GetMouseButton(0))
        {
            dragSelectionTimer += Time.deltaTime;

            if (dragSelectionTimer >= dragSelectionDelay)
            {
                isDragging = true;
                UpdateSelectionBox(Input.mousePosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragSelectionTimer = 0f;
            isDragging = false;

            SelectObjectsInRectangle();
        }
    }
}
