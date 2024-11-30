using Unity.Netcode;
using UnityEngine;

public enum CursorType
{
    Default,
    Attack,
    Build,
    Heal,
    Gather
}

public class CursorManager : NetworkBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D buildCursor;
    [SerializeField] private Texture2D healCursor;
    [SerializeField] private Texture2D gatherCursor;
    [SerializeField] private Vector2 cursorOffset = new(10, 4);
    private PlayerController playerController;
    private SelectionManager selectionManager;
    private CursorType currentCursorType;

    private void Start()
    {
        if (!IsOwner) { enabled = false; return; }
        playerController = GetComponent<PlayerController>();
        selectionManager = GetComponent<SelectionManager>();

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

    public void SetGatherCursor()
    {
        Cursor.SetCursor(gatherCursor, cursorOffset, CursorMode.Auto);
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
            return damagable != null && !damagable.isDead.Value && damagable.teamType.Value != playerController.teamType.Value && unit.isVisibile.Value;
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

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, 100f);

        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("ForceField")) continue;

            var damagable = hit.collider.gameObject.GetComponent<Damagable>();

            if (selectionManager.selectedObjects.Count == 1 && damagable == null && damagable == selectionManager.selectedObjects[0].GetComponent<Damagable>())
            {
                return false;
            }

            if (damagable != null && !damagable.isDead.Value && damagable.teamType.Value == playerController.teamType.Value && damagable.stats.GetStat(StatType.Health) < damagable.stats.GetStat(StatType.MaxHealth))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsGatheringHovering()
    {
        if (selectionManager.GetWorkers().Count == 0) return false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f);

        if (isHit)
        {
            var resource = hit.collider.gameObject.GetComponent<GatherItem>();
            return resource != null;
        }
        else
        {
            return false;
        }
    }

    private void SetCursor(CursorType cursorType)
    {
        if (currentCursorType == cursorType) return;

        switch (cursorType)
        {
            case CursorType.Default:
                SetDefaultCursor();
                break;
            case CursorType.Attack:
                SetAttackCursor();
                break;
            case CursorType.Build:
                SetBuildCursor();
                break;
            case CursorType.Heal:
                SetHealCursor();
                break;
            case CursorType.Gather:
                SetGatherCursor();
                break;
        }

        currentCursorType = cursorType;
    }

    private void FixedUpdate()
    {
        if (IsEnemyHovering())
        {
            SetCursor(CursorType.Attack);
        }
        else if (IsConstructionHovering())
        {
            SetCursor(CursorType.Build);
        }
        else if (IsHealHovering())
        {
            SetCursor(CursorType.Heal);
        }
        else if (IsGatheringHovering())
        {
            SetCursor(CursorType.Gather);
        }
        else
        {
            SetCursor(CursorType.Default);
        }
    }
}
