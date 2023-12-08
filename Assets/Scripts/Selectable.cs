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

    // when unit killed should be diselected
    private void OnDestroy()
    {
        if (isSelected)
        {
            SelectionManager.Instance.selectedObjects.Remove(this);
            Deselect();
        }
    }

    public void Select()
    {
        Debug.Log("Select");
        isSelected = true;
        if (selectionCircle == null) return;
        selectionCircle.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        isSelected = false;
        if (selectionCircle == null) return;
        selectionCircle.gameObject.SetActive(false);
    } 
}
