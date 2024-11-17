using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitCountManager : NetworkToolkitHelper
{
    [SerializeField] private int maxUnitCount = 20;
    [SerializeField] private int currentUnitCount = 0;

    private Label unitCountText;
    private RTSObjectsManager RTSObjectsManager;

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
        RTSObjectsManager = GetComponentInParent<RTSObjectsManager>();
        root = UIDocument.rootVisualElement;
        unitCountText = root.Q<Label>("UnitCountLabel");
        Debug.Log("UnitCountManager Start");
        RTSObjectsManager.OnUnitChange += OnUnitChange;
    }

    private void OnDisable()
    {
        RTSObjectsManager.OnUnitChange -= OnUnitChange;
    }

    public bool CanSpawnUnit()
    {
        return currentUnitCount < maxUnitCount;
    }

    private void OnUnitChange(Unit unit, List<Unit> units)
    {
        currentUnitCount = units.Count;
        UpdateText();
    }

    private void UpdateText()
    {
        unitCountText.text = $"{currentUnitCount}/{maxUnitCount}";
    }
}
