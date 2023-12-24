using UnityEngine;
using UnityEngine.EventSystems;

public class RTSManager : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    public float unitSpacing = 0.3f;

    private void CancelBuildingCommand(Selectable selectable) {
        var workerScript = selectable.GetComponent<Worker>();

        if (workerScript != null) {
            workerScript.StopConstruction();
        }
    }

    private void MoveCommand(Vector3 position) {
        int unitsCount = SelectionManager.Instance.selectedObjects.Count;
        int rows = Mathf.CeilToInt(Mathf.Sqrt(unitsCount));
        int cols = Mathf.CeilToInt((float)unitsCount / rows);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var index = row * cols + col;
                var unit = SelectionManager.Instance.selectedObjects[index];
                if (unit.selectableType == Selectable.SelectableType.Unit) {
                    if (index < unitsCount)
                    {
                        Vector3 offset = new Vector3(col * unitSpacing, 0f, row * unitSpacing);
                        Vector3 finalPosition = position + offset;
                        var unitMovement = SelectionManager.Instance.selectedObjects[index].GetComponent<UnitMovement>();
                        
                        finalPosition += unitMovement.agent.radius * 2.0f * col * transform.right;
                        finalPosition += unitMovement.agent.radius * 2.0f * row * transform.forward;
                        // draw point on the ground
                        // GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        // point.transform.position = finalPosition;
                        // point.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        // point.GetComponent<MeshRenderer>().material.color = Color.red;
                        unitMovement.MoveTo(finalPosition);
                        CancelBuildingCommand(unit);
                    }
                }
            }
        }
    }

    private void SetTarget(Damagable target, Selectable selectable) {
        var attackScript = selectable.GetComponent<Attack>();

        if (attackScript != null) {
            attackScript.SetTarget(target);
            CancelBuildingCommand(selectable);
        }
    }

    private void AttackCommand(Damagable target) {
        foreach (Selectable selectable in SelectionManager.Instance.selectedObjects) {
            var unitScript = selectable.GetComponent<Unit>();
            var distance = Vector3.Distance(target.transform.position, selectable.transform.position);
            
            if (unitScript != null && unitScript.attackableSo.attackRange < distance) {
                // Move to target
                var unitMovement = selectable.GetComponent<UnitMovement>();
                var offsetPoint = 2f;
                // calculete the closest point to be in range offset indicates that unit should be closer to target by offset
                var closestPointToBeInRange = target.transform.position + (selectable.transform.position - target.transform.position).normalized * (distance - unitScript.attackableSo.attackRange + offsetPoint);
                
                unitMovement.MoveTo(closestPointToBeInRange);
                SetTarget(target, selectable);
                continue;
            }

            SetTarget(target, selectable);
        }
    }

    private void BuildCommand(Construction construction) {
        var workers = SelectionManager.Instance.GetWorkers();

        foreach (var worker in workers) {
            var workerScript = worker.GetComponent<Worker>();

            if (workerScript != null) {
                // if is clicked on building that worker currently building
                if (workerScript.construction == construction) {
                    return;
                }
                // if worker is building something else
                if (workerScript.construction != null) {
                    workerScript.StopConstruction();
                }
                // move to construction
                workerScript.MoveToConstruction(construction);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && SelectionManager.Instance.selectedObjects.Count > 0 && !UIHelper.Instance.IsPointerOverUIElement())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.point != null)
            {
                var damagableScript = hit.transform.gameObject.GetComponent<Damagable>();
                var selectableScript = hit.transform.gameObject.GetComponent<Selectable>();
                var constructionScript = hit.transform.gameObject.GetComponent<Construction>();

                if (damagableScript != null && selectableScript != null) {
                    // Attack
                    if (damagableScript.playerId != PlayerController.Instance.playerId) {
                        AttackCommand(damagableScript);
                        return;
                    }
                    // ------------------------------------------------
                    if (selectableScript.selectableType == Selectable.SelectableType.Building && damagableScript.playerId == PlayerController.Instance.playerId && constructionScript != null) {
                        // Build
                        BuildCommand(constructionScript);
                        return;
                    }
                }

                MoveCommand(hit.point);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Instantiate(unitPrefab, new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f)), Quaternion.identity);
        }   
    }
}
