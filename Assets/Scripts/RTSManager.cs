using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RTSManager : NetworkBehaviour
{
    [SerializeField] private GameObject moveIndicatorPrefab;
    private PlayerController playerController;
    private SelectionManager selectionManager;
    private UIRTSActions uiRTSActions;
    public float unitSpacing = 0.2f;

    private void Start()
    {
        if (!IsLocalPlayer)
        {
            enabled = false;
            return;
        }

        playerController = GetComponent<PlayerController>();
        selectionManager = GetComponent<SelectionManager>();
        uiRTSActions = playerController.toolbar.GetComponent<UIRTSActions>();
    }

    private void CancelBuildingCommand(Selectable selectable)
    {
        var workerScript = selectable.GetComponent<Worker>();

        if (workerScript != null)
        {
            workerScript.StopConstruction();
        }
    }

    private void CancelHealingCommand(Selectable selectable)
    {
        var healerScript = selectable.GetComponent<Healer>();

        if (healerScript != null)
        {
            healerScript.SetTargetToNullServerRpc();
        }
    }

    private void MoveCommand(Vector3 position)
    {
        int unitsCount = selectionManager.selectedObjects.Count;
        Debug.Log($"unitsCount: {unitsCount}");
        int rows = Mathf.CeilToInt(Mathf.Sqrt(unitsCount));
        int cols = Mathf.CeilToInt((float)unitsCount / rows);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var index = row * cols + col;
                if (index >= unitsCount) continue;
                var unit = selectionManager.selectedObjects[index];
                if (unit.selectableType == Selectable.SelectableType.Unit)
                {
                    Vector3 offset = new Vector3(col * unitSpacing, 0f, row * unitSpacing);
                    Vector3 finalPosition = position + offset;
                    var unitMovement = selectionManager.selectedObjects[index].GetComponent<UnitMovement>();

                    finalPosition += unitMovement.agent.radius * 2.0f * col * transform.right;
                    finalPosition += unitMovement.agent.radius * 2.0f * row * transform.forward;

                    unitMovement.MoveToServerRpc(finalPosition);
                    CancelBuildingCommand(unit);
                    CancelHealingCommand(unit);
                }
            }
        }

        var go = Instantiate(moveIndicatorPrefab, position, Quaternion.identity);
        Destroy(go, 2f);
    }

    private void SetTarget(Damagable target, Selectable selectable)
    {
        var attackScript = selectable.GetComponent<Attack>();

        if (attackScript != null)
        {
            var no = target.GetComponent<NetworkObject>();
            attackScript.SetTargetServerRpc(no);
            CancelBuildingCommand(selectable);
        }
    }

    private void AttackCommand(Damagable target)
    {
        foreach (Selectable selectable in selectionManager.selectedObjects)
        {
            var unitScript = selectable.GetComponent<Unit>();
            var distance = Vector3.Distance(target.transform.position, selectable.transform.position);

            if (unitScript != null && unitScript.attackableSo.attackRange < distance)
            {
                // Move to target
                var unitMovement = selectable.GetComponent<UnitMovement>();
                var offsetPoint = 2f;
                var directionToTarget = (target.transform.position - selectable.transform.position).normalized;

                // Calculate the closest point to be in range with the specified offset
                var closestPointToBeInRange = target.transform.position - directionToTarget * (unitScript.attackableSo.attackRange - offsetPoint);

                unitMovement.MoveToServerRpc(closestPointToBeInRange);
                SetTarget(target, selectable);
                continue;
            }

            SetTarget(target, selectable);
        }
    }

    private void BuildCommand(Construction construction)
    {
        var workers = selectionManager.GetWorkers();

        foreach (var worker in workers)
        {
            var workerScript = worker.GetComponent<Worker>();

            if (workerScript != null)
            {
                // if is clicked on building that worker currently building
                if (workerScript.construction == construction)
                {
                    return;
                }
                // if worker is building something else
                if (workerScript.construction != null)
                {
                    workerScript.StopConstruction();
                }
                // move to construction
                workerScript.MoveToConstruction(construction);
            }
        }
    }

    private void HealCommand(List<Selectable> healers, Damagable target)
    {
        foreach (var healer in healers)
        {
            var healerScript = healer.GetComponent<Healer>();
            var damagableScript = healer.GetComponent<Damagable>();

            if (damagableScript == target)
            {
                continue;
            }

            if (healerScript != null)
            {
                var no = target.GetComponent<NetworkObject>();
                healerScript.SetTargetServerRpc(no);
            }
        }
    }

    void Update()
    {
        Debug.Log($"RTS MANAGER {selectionManager.selectedObjects.Count}");
        if (Input.GetMouseButtonDown(1) && selectionManager.selectedObjects.Count > 0 && !UIHelper.Instance.IsPointerOverUIElement())
        {
            if (uiRTSActions.isSetTargetMode)
            {
                return;
            }

            RaycastHit[] raycastHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity);
            bool isAction = false;

            foreach (var raycastHit in raycastHits)
            {
                if (raycastHit.transform.gameObject.CompareTag("ForceField")) continue;
                Debug.Log(raycastHit.transform.gameObject.name);
                var damagableScript = raycastHit.transform.gameObject.GetComponent<Damagable>();
                var selectableScript = raycastHit.transform.gameObject.GetComponent<Selectable>();
                var constructionScript = raycastHit.transform.gameObject.GetComponent<Construction>();

                Debug.Log(OwnerClientId + "RTS MANAGER");
                if (damagableScript != null && selectableScript != null)
                {
                    // Attack
                    if (damagableScript.OwnerClientId != playerController.OwnerClientId)
                    {
                        AttackCommand(damagableScript);
                        isAction = true;
                        return;
                    }
                    // ------------------------------------------------
                    if (selectableScript.selectableType == Selectable.SelectableType.Building && damagableScript.OwnerClientId == playerController.OwnerClientId && constructionScript != null)
                    {
                        // Build
                        BuildCommand(constructionScript);
                        isAction = true;
                        return;
                    }

                    // Heal 
                    if (damagableScript.OwnerClientId == playerController.OwnerClientId && damagableScript.stats.GetStat(StatType.Health) < damagableScript.stats.GetStat(StatType.MaxHealth))
                    {
                        var healers = selectionManager.GetHealers();

                        HealCommand(healers, damagableScript);
                        isAction = true;
                        return;
                    }
                }
            }
            // Move
            if (!isAction)
            {
                if (raycastHits.Length > 0)
                {
                    MoveCommand(raycastHits[0].point);
                }
            }
        }
    }
}
