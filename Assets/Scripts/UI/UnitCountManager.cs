using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitCountManager : MonoBehaviour
{
    [SerializeField] private int maxUnitCount = 20;
    [SerializeField] private int currentUnitCount = 0;
    [SerializeField] private TextMeshProUGUI unitCountText;
    public static UnitCountManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerController.OnUnitChange += OnUnitChange;
    }

    private void OnDisable() {
        PlayerController.OnUnitChange -= OnUnitChange;
    }

    public bool CanSpawnUnit() {
        return currentUnitCount < maxUnitCount;
    }

    private void OnUnitChange(Unit unit, List<Unit> units) {
        currentUnitCount = units.Count;
        UpdateText();
    }

    private void UpdateText() {
        Debug.Log($"Current unit count: {currentUnitCount}");
        unitCountText.text = $"{currentUnitCount}/{maxUnitCount}";
    }
}
