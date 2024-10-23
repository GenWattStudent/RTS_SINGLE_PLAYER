using System.Collections.Generic;
using RTS.Domain.SO;
using RTS.Managers;
using Unity.Netcode;
using UnityEngine;

public class RTSManager : NetworkBehaviour
{
    [SerializeField] private GameObject moveIndicatorPrefab;
    public float unitSpacing = 0.2f;

    private PlayerController playerController;
    private SelectionManager selectionManager;
    private UIRTSActions uiRTSActions;
    private UpgradeManager upgradeManager;
    private UIStorage uiStorage;

    private void Start()
    {
        if (!IsLocalPlayer)
        {
            enabled = false;
            return;
        }

        playerController = GetComponent<PlayerController>();
        selectionManager = GetComponent<SelectionManager>();
        uiRTSActions = GetComponentInChildren<UIRTSActions>();
        upgradeManager = GetComponent<UpgradeManager>();
        uiStorage = GetComponentInChildren<UIStorage>();
    }

    private void CancelBuildingCommand(Selectable selectable)
    {
        var workerScript = selectable.GetComponent<Worker>();

        if (workerScript != null)
        {
            workerScript.StopConstructionServerRpc();
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

    private List<Vector3> GetRectangleFormationPositions(int unitCount, int ColumnCount, bool centerUnits = true)
    {
        List<Vector3> unitPositions = new List<Vector3>();
        var unitsPerRow = Mathf.Min(ColumnCount, unitCount);
        float offsetX = (unitsPerRow - 1) * unitSpacing / 2f;

        if (unitsPerRow == 0)
        {
            return new List<Vector3>();
        }

        float rowCount = unitCount / ColumnCount + (unitCount % ColumnCount > 0 ? 1 : 0);
        float x, y, column;
        int firstIndexInRow;

        for (int row = 0; unitPositions.Count < unitCount; row++)
        {
            // Check if centering is enabled and if row has less than maximum
            // allowed units within the row.
            firstIndexInRow = row * ColumnCount;
            if (centerUnits &&
                row != 0 &&
                firstIndexInRow + ColumnCount > unitCount)
            {
                // Alter the offset to center the units that do not fill the row
                var emptySlots = firstIndexInRow + ColumnCount - unitCount;
                offsetX -= emptySlots / 2f * unitSpacing;
            }

            for (column = 0; column < ColumnCount; column++)
            {
                if (firstIndexInRow + column < unitCount)
                {
                    x = column * unitSpacing - offsetX;
                    y = row * unitSpacing;

                    Vector3 newPosition = new Vector3(x, 0, -y);
                    unitPositions.Add(newPosition);
                }
                else
                {
                    return unitPositions;
                }
            }
        }

        return unitPositions;
    }

    private void MoveCommand(Vector3 position)
    {
        int unitsCount = selectionManager.selectedObjects.Count;
        Debug.Log($"unitsCount: {unitsCount}");
        int rows = Mathf.CeilToInt(Mathf.Sqrt(unitsCount));
        int cols = Mathf.CeilToInt((float)unitsCount / rows);
        var positions = GetRectangleFormationPositions(unitsCount, cols);

        for (int i = 0; i < unitsCount; i++)
        {
            var unit = selectionManager.selectedObjects[i];
            if (unit.selectableType == Selectable.SelectableType.Unit)
            {
                var unitMovement = selectionManager.selectedObjects[i].GetComponent<UnitMovement>();
                unitMovement.MoveToServerRpc(position + positions[i]);
                CancelBuildingCommand(unit);
                CancelHealingCommand(unit);
            }
        }

        var go = Instantiate(moveIndicatorPrefab, new Vector3(position.x, position.y + 0.01f, position.z), moveIndicatorPrefab.transform.rotation);
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
            var no = construction.GetComponent<NetworkObject>();

            if (workerScript != null)
            {
                // move to construction
                workerScript.MoveToConstructionServerRpc(no);
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

    [ServerRpc]
    private void UpgradeCommandServerRpc(NetworkObjectReference no, int index)
    {
        var upgrade = upgradeManager.Upgrades[index];

        if (no.TryGet(out NetworkObject networkObject))
        {
            var unit = networkObject.GetComponent<Unit>();
            if (!upgradeManager.CanApplyUpgrade(unit) && !uiStorage.HasEnoughResource(upgrade.costResource, upgrade.Cost)) return;

            unit.IsUpgrading = true;
            unit.AddUpgrade(upgrade);

            var upgradeGo = Instantiate(upgrade.constructionManagerPrefab, unit.transform.position, Quaternion.identity);
            var upgradeNo = upgradeGo.GetComponent<NetworkObject>();
            var damagable = upgradeGo.GetComponent<Damagable>();
            var stats = upgradeGo.GetComponent<Stats>();
            var construction = upgradeGo.GetComponent<Construction>();

            construction.construction = upgrade;
            upgradeNo.SpawnWithOwnership(OwnerClientId);
            upgradeNo.transform.SetParent(unit.transform);
            damagable.teamType.Value = playerController.teamType.Value;
            stats.AddStat(StatType.Health, 1);

            uiStorage.DecreaseResource(upgrade.costResource, upgrade.Cost);
        }
    }

    private void UpgradeCommand(Unit unit, UpgradeSO upgrade)
    {
        UpgradeCommandServerRpc(unit.GetComponent<NetworkObject>(), upgradeManager.Upgrades.IndexOf(upgrade));
    }

    private void Update()
    {
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
                var unitScript = raycastHit.transform.gameObject.GetComponent<Unit>();

                if (damagableScript != null && !damagableScript.IsBot && selectableScript != null)
                {
                    // Attack
                    if (damagableScript.teamType.Value != playerController.teamType.Value && unitScript.isVisibile)
                    {
                        Debug.Log("AttackCommand " + damagableScript.teamType + " " + playerController.teamType);
                        AttackCommand(damagableScript);
                        isAction = true;
                        return;
                    }
                    // ------------------------------------------------
                    if (selectableScript.selectableType == Selectable.SelectableType.Building && damagableScript.OwnerClientId == playerController.OwnerClientId && constructionScript != null)
                    {
                        // Build
                        Debug.Log("BuildCommand");
                        BuildCommand(constructionScript);
                        isAction = true;
                        return;
                    }

                    // Heal 
                    if (damagableScript.teamType.Value == playerController.teamType.Value && damagableScript.stats.GetStat(StatType.Health) < damagableScript.stats.GetStat(StatType.MaxHealth))
                    {
                        var healers = selectionManager.GetHealers();
                        Debug.Log("HealCommand " + damagableScript.OwnerClientId + " " + playerController.OwnerClientId);
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

        if (Input.GetMouseButtonDown(0) && !UIHelper.Instance.IsPointerOverUIElement())
        {
            // Raycast hit unit or building
            RaycastHit raycastHit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, Mathf.Infinity))
            {
                var unit = raycastHit.transform.gameObject.GetComponent<Unit>();

                if (upgradeManager.CanApplyUpgrade(unit))
                {
                    Debug.Log("Upgrade");
                    UpgradeCommand(unit, upgradeManager.SelectedUpgrade);
                }
            }

            upgradeManager.SelectUpgrade(null);
        }
    }
}
