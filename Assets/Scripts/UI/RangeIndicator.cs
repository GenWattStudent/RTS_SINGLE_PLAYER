using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    private Unit unit;
    private Selectable selectable;

    void Start()
    {
        selectable = GetComponentInParent<Selectable>();
        unit = GetComponentInParent<Unit>();

        transform.localScale = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
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
