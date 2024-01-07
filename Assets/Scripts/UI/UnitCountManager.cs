using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitCountManager : MonoBehaviour
{
    private UIDocument UIDocument;
    private VisualElement root;
    private Label unitCountText;
    [SerializeField] private int maxUnitCount = 20;
    [SerializeField] private int currentUnitCount = 0;
    public static UnitCountManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        unitCountText = root.Q<Label>("UnitCountLabel");
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
