using UnityEngine;

public class RTSManager : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;

    private void CancelBuildingCommand(Selectable selectable) {
        var workerScript = selectable.GetComponent<Worker>();

        if (workerScript != null) {
            workerScript.StopConstruction();
        }
    }

    private void MoveCommand(Vector3 position) {
        int index = 0;

        foreach (Selectable selectable in SelectionManager.Instance.selectedObjects)
        {
            if (selectable.selectableType == Selectable.SelectableType.Unit)
            {
                var moveScript = selectable.GetComponent<UnitMovement>();
                var agent = selectable.GetComponent<UnityEngine.AI.NavMeshAgent>();
                // calculate position for each unit take navmesh radius into accoun
                var unitPosition = new Vector3(position.x + index * (agent.radius + 0.5f), position.y, position.z + index * (agent.radius + 0.55f));

                if (moveScript != null) {
                    moveScript.MoveTo(unitPosition);
                    CancelBuildingCommand(selectable);
                }
                index++;
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

            if (Physics.Raycast(ray, out RaycastHit hit))
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
                    Debug.Log(selectableScript.selectableType);
                    if (selectableScript.selectableType == Selectable.SelectableType.Building && damagableScript.playerId == PlayerController.Instance.playerId && constructionScript != null) {
                        // Build
                        BuildCommand(constructionScript);
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
