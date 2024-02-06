using Unity.Netcode;
using UnityEngine;

public class CursorManager : NetworkBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D buildCursor;
    [SerializeField] private Texture2D healCursor;
    [SerializeField] private Vector2 cursorOffset = new(10, 4);
    public ulong clientId;

    private void Start()
    {
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
        if (SelectionManager.selectedObjects.Count == 0 && !SelectionManager.IsCanAttack()) return false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f);

        if (isHit)
        {
            var damagable = hit.collider.gameObject.GetComponent<Damagable>();
            var playerController = PlayerController.Instance.GetPlayerControllerWithClientId(clientId);
            return damagable != null && !damagable.isDead && damagable.OwnerClientId != playerController.OwnerClientId;
        }
        else
        {
            return false;
        }
    }

    public bool IsConstructionHovering()
    {
        if (SelectionManager.selectedObjects.Count == 0 && SelectionManager.GetWorkers().Count == 0) return false;

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
        if (SelectionManager.GetHealers().Count == 0) return false;

        if (SelectionManager.selectedObjects.Count == 1)
        {
            var healer = SelectionManager.selectedObjects[0].GetComponent<Healer>();
            if (healer != null)
            {
                return false;
            }
        }
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, 100f);
        var playerController = PlayerController.Instance.GetPlayerControllerWithClientId(clientId);

        foreach (var hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("ForceField")) continue;

            var damagable = hit.collider.gameObject.GetComponent<Damagable>();

            if (damagable != null && !damagable.isDead && damagable.OwnerClientId == playerController.OwnerClientId && damagable.stats.GetStat(StatType.Health) < damagable.stats.GetStat(StatType.MaxHealth))
            {
                return true;
            }
        }

        return false;
    }

    private void FixedUpdate()
    {
        if (IsEnemyHovering())
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
