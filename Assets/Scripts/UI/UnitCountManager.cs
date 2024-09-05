using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitCountManager : NetworkToolkitHelper
{
    private Label unitCountText;
    [SerializeField] private int maxUnitCount = 20;
    [SerializeField] private int currentUnitCount = 0;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        unitCountText = root.Q<Label>("UnitCountLabel");
        RTSObjectsManager.OnUnitChange += OnUnitChange;
    }

    private void OnDisable()
    {
        if (!IsOwner) return;
        RTSObjectsManager.OnUnitChange -= OnUnitChange;
    }

    public bool CanSpawnUnit()
    {
        return currentUnitCount < maxUnitCount;
    }

    private void OnUnitChange(Unit unit, List<Unit> units)
    {
        if (!IsOwner) return;
        currentUnitCount = units.Count;
        UpdateText();
    }

    private void UpdateText()
    {
        unitCountText.text = $"{currentUnitCount}/{maxUnitCount}";
    }
}
