using UnityEngine;

// This class is used to identify objects that can be selected by the player. It can be building or units.
public class Selectable : MonoBehaviour
{
    public enum SelectableType
    {
        Building,
        Unit
    }

    [SerializeField] private RectTransform selectionCircle;
    public SelectableType selectableType;
    [HideInInspector] public bool isSelected = false;
    private Damagable damagable;
    private Camera unitCamera;

    // when unit killed should be diselected
    private void Awake() {
        damagable = GetComponent<Damagable>();
        unitCamera = GetComponentInChildren<Camera>(true);
    }

    private void OnDead() {
        SelectionManager.Deselect(this);
        Deselect();
    }

    public void Select()
    {
        isSelected = true;
        damagable.OnDead += OnDead;
        if (selectionCircle == null) return;
        selectionCircle.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        isSelected = false;
        if (unitCamera != null) unitCamera.gameObject.SetActive(false);
        damagable.OnDead -= OnDead;
        if (selectionCircle == null) return;
        selectionCircle.gameObject.SetActive(false);
    } 
}
