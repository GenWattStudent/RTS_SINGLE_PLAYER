using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D buildCursor;

    private void Start() {
        SetDefaultCursor();
    }

    public void SetDefaultCursor() {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetAttackCursor() {
        Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetBuildCursor() {
        Cursor.SetCursor(buildCursor, Vector2.zero, CursorMode.Auto);
    }

    public bool IsEnemyHovering() {
        if (SelectionManager.Instance.selectedObjects.Count == 0 && SelectionManager.Instance.IsCanAttack()) return false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f);

        if (isHit) {
            var damagable = hit.collider.gameObject.GetComponent<Damagable>();
            return damagable != null && !damagable.isDead && damagable.playerId != PlayerController.Instance.playerId;
        } else {
            return false;
        }
    }

    public bool IsConstructionHovering() {
        if (SelectionManager.Instance.selectedObjects.Count == 0 && SelectionManager.Instance.GetWorkers().Count == 0) return false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var isHit = Physics.Raycast(ray, out hit, 1000f);

        if (isHit) {
            var building = hit.collider.gameObject.GetComponent<Construction>();
            return building != null;
        } else {
            return false;
        }
    }

    private void FixedUpdate() {
        if (IsEnemyHovering()) {
            SetAttackCursor();
        } else if (IsConstructionHovering()) {
            SetBuildCursor();
        } else {
            SetDefaultCursor();
        }
    }
}
