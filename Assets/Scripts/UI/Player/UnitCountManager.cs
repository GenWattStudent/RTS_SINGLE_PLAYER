using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitCountManager : NetworkToolkitHelper
{
    [SerializeField] private int maxUnitCount = 20;
    [SerializeField] private int currentUnitCount = 0;

    private Label unitCountText;
    private RTSObjectsManager RTSObjectsManager;

    protected override void OnEnable()
    {
        base.OnEnable();
        UIDocument = GetComponent<UIDocument>();
        RTSObjectsManager = GetComponentInParent<RTSObjectsManager>();
        root = UIDocument.rootVisualElement;
        unitCountText = root.Q<Label>("UnitCountLabel");
        RTSObjectsManager.OnUnitChange += OnUnitChange;
    }

    private void OnDisable()
    {
        RTSObjectsManager.OnUnitChange -= OnUnitChange;
    }

    public bool CanSpawnUnit(int unitsToSpawn)
    {
        return currentUnitCount + unitsToSpawn <= maxUnitCount;
    }

    private void OnUnitChange(Unit unit, List<Unit> units)
    {
        currentUnitCount = units.Count;

        if (!IsOwner) return;
        UpdateText();
    }

    private void UpdateText()
    {
        unitCountText.text = $"{currentUnitCount}/{maxUnitCount}";
    }
}
