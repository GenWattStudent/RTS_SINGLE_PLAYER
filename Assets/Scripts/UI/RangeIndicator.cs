using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private Unit unit;
    private Selectable selectable;

    private void OnEnable()
    {
        selectable = GetComponentInParent<Selectable>();
        selectable.OnSelected += ShowRange;
    }

    private void Start()
    {
        unit = GetComponentInParent<Unit>();

        transform.localScale = new Vector3(0, 0, 0);
    }

    private void ShowRange(Selectable selectable)
    {
        if (selectable.isSelected)
        {
            transform.localScale = new Vector3(unit.attackableSo.attackRange * 2, .2f, unit.attackableSo.attackRange * 2);
        }
        else
        {
            transform.localScale = new Vector3(0, 0, 0);
        }
    }
}
