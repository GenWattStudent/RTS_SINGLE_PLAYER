using RTS.Managers;
using Unity.Netcode;
using UnityEngine;

public class CursorManager : NetworkBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D buildCursor;
    [SerializeField] private Texture2D healCursor;
    [SerializeField] private Vector2 cursorOffset = new(10, 4);
    private PlayerController playerController;
    private SelectionManager selectionManager;
    private UpgradeManager upgradeManager;

    private void Start()
    {
        if (!IsOwner) { enabled = false; return; }
        playerController = GetComponent<PlayerController>();
        selectionManager = GetComponent<SelectionManager>();
        upgradeManager = GetComponent<UpgradeManager>();

        SetDefaultCursor();
    }

    public void SetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, cursorOffset, CursorMode.Auto);
    }

    public void SetAttackCursor()
    {
        Cursor.SetCursor(attackCursor, cursorOffset, CursorMode.Auto);
    }

    public void SetBuildCursor()
    {
        Cursor.SetCursor(buildCursor, cursorOffset, CursorMode.Auto);
    }

    public void SetHealCursor()
    {
        Cursor.SetCursor(healCursor, cursorOffset, CursorMode.Auto);
    }

    public bool IsEnemyHovering()
    {
        if (selectionManager.selectedObjects.Count == 0 || !selectionManager.IsCanAttack()) return false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f);

        if (isHit)
        {
            var damagable = hit.collider.gameObject.GetComponent<Damagable>();
            var unit = hit.collider.gameObject.GetComponent<Unit>();
            return damagable != null && !damagable.isDead && damagable.teamType.Value != playerController.teamType.Value && unit.isVisibile;
        }
        else
        {
            return false;
        }
    }

    public bool IsConstructionHovering()
    {
        if (selectionManager.selectedObjects.Count == 0 || selectionManager.GetWorkers().Count == 0) return false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f);

        if (isHit)
        {
            var building = hit.collider.gameObject.GetComponent<Construction>();
            return building != null;
        }
        else
        {
            return false;
        }
    }

    public bool IsHealHovering()
    {
        if (selectionManager.GetHealers().Count == 0) return false;

        if (selectionManager.selectedObjects.Count == 1)
        {
            var healer = selectionManager.selectedObjects[0].GetComponent<Healer>();
            if (healer != null)
            {
                return false;
            }
        }
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, 100f);

        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("ForceField")) continue;

            var damagable = hit.collider.gameObject.GetComponent<Damagable>();

            if (damagable != null && !damagable.isDead && damagable.teamType.Value == playerController.teamType.Value && damagable.stats.GetStat(StatType.Health) < damagable.stats.GetStat(StatType.MaxHealth))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsUpgradeSelected()
    {
        return upgradeManager.SelectedUpgrade != null;
    }

    private void FixedUpdate()
    {
        if (IsUpgradeSelected())
        {
            SetBuildCursor();
        }
        else if (IsEnemyHovering())
        {
            SetAttackCursor();
        }
        else if (IsConstructionHovering())
        {
            SetBuildCursor();
        }
        else if (IsHealHovering())
        {
            SetHealCursor();
        }
        else
        {
            SetDefaultCursor();
        }
    }
}
