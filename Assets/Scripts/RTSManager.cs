using UnityEngine;
using UnityEngine.AI;

public class RTSManager : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    public float unitSpacing = 1;

    private void CancelBuildingCommand(Selectable selectable) {
        var workerScript = selectable.GetComponent<Worker>();
        Debug.Log("Cancel building command");
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
                    Debug.Log("Move command " + index);
                    if (index < unitsCount)
                    {
                        Vector3 offset = new Vector3(col * unitSpacing, 0f, row * unitSpacing);
                        Vector3 finalPosition = position + offset;
                        var unitMovement = SelectionManager.Instance.selectedObjects[index].GetComponent<UnitMovement>();
                        
                        finalPosition += unitMovement.agent.radius * 2.0f * col * transform.right;
                        finalPosition += unitMovement.agent.radius * 2.0f * row * transform.forward;
                        Debug.Log("Move command " + finalPosition);
                        unitMovement.MoveTo(finalPosition);
                    }
                }
            }
        }
    }

    private void AttackCommand(Damagable target) {
        foreach (Selectable selectable in SelectionManager.Instance.selectedObjects) {
            var attackScript = selectable.GetComponent<Attack>();

            if (attackScript != null) {
                attackScript.target = target;
                CancelBuildingCommand(selectable);
            }
        }
    }

    private void BuildCommand(Construction construction) {
        var workers = SelectionManager.Instance.GetWorkers();
        Debug.Log(construction + "Count " + workers.Count);
        foreach (var worker in workers) {
            var workerScript = worker.GetComponent<Worker>();

            if (workerScript != null) {
                workerScript.MoveToConstruction(construction);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && SelectionManager.Instance.selectedObjects.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.point != null)
            {
                var damagableScript = hit.transform.gameObject.GetComponent<Damagable>();
                var unitScript = hit.transform.gameObject.GetComponent<Unit>();
                var selectableScript = hit.transform.gameObject.GetComponent<Selectable>();
                var constructionScript = hit.transform.gameObject.GetComponent<Construction>();

                if (damagableScript != null && unitScript != null && selectableScript != null) {
                    // Attack
                    if (damagableScript.playerId != PlayerController.Instance.playerId) {
                        var distance = Vector3.Distance(hit.point, transform.position);

                       if (distance <= unitScript.unitSo.attackRange) {
                            AttackCommand(damagableScript);
                            return;
                        }
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
