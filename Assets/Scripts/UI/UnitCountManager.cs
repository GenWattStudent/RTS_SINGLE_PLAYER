using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitCountManager : NetworkBehaviour
{
    private UIDocument UIDocument;
    private VisualElement root;
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

    private void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        unitCountText = root.Q<Label>("UnitCountLabel");
        PlayerController.OnUnitChange += OnUnitChange;
    }

    private void OnDisable()
    {
        if (!IsOwner) return;
        PlayerController.OnUnitChange -= OnUnitChange;
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
