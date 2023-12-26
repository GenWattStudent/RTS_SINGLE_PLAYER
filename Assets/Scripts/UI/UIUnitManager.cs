using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitManager : MonoBehaviour
{
    [SerializeField] private GameObject unitSlotTabPrefab;
    // [SerializeField] private Button upgradeButton;
    private List<GameObject> unitSlotTabs = new ();
    private List<UnitSo> unitsAttachedToTab = new ();
    public static UIUnitManager Instance { get; private set; }
    private BuildingSo selectedBuilding;
    private ISpawnerBuilding spawnerBuilding;
    public GameObject currentBuilding;
    public bool IsUnitUIOpen { get; set; } = false;
    public bool IsUnitSelectionTabOpen { get; set; } = false;
    private Dictionary<string, List<UnitSo>> unitQueue = new ();
    private int unitCountPrev = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start() {
        SelectionManager.Instance.OnSelect += CreateSelectionUnitTab;
    }
    
    private void OnDisable() {
        SelectionManager.Instance.OnSelect -= CreateSelectionUnitTab;
    }

    // private void HandleUpgrade() {
    //     var levelableObject = curentBuilding.GetComponent<LevelableObject>();

    //     if (levelableObject != null) {
    //         levelableObject.LevelUp();
    //     }
    // }

    // private void OnEnable() {
    //     upgradeButton.onClick.AddListener(HandleUpgrade);
    // }

    // private void OnDisable() {
    //     upgradeButton.onClick.RemoveListener(HandleUpgrade);
    // }

    private void FixedUpdate() {
        if (!IsUnitSelectionTabOpen && IsUnitUIOpen && spawnerBuilding is not null) {
            foreach (var unitTab in unitSlotTabs) {
                var nameText = unitTab.GetComponentsInChildren<TextMeshProUGUI>()[0];
                var currentTime = spawnerBuilding.GetSpawnTimer();
                var currentSpawningUnit = spawnerBuilding.GetCurrentSpawningUnit();
                var unitQueueCount = spawnerBuilding.GetUnitQueueCountByName(unitTab.name);
                var soUnit = unitsAttachedToTab.Find(x => x.unitName == unitTab.name);
    
                if (currentSpawningUnit is not null && currentSpawningUnit.unitName == unitTab.name) {
                    var spawnPanel = unitTab.GetComponentInChildren<SpawnPanel>(true);
                    spawnPanel.SetSpawnData(unitQueueCount, currentTime, spawnerBuilding.totalSpawnTime);
                }

                if (unitQueueCount <= 0) {
                    var spawnPanel = unitTab.GetComponentInChildren<SpawnPanel>(true);
                    if (!spawnPanel) return;
                    spawnPanel.gameObject.SetActive(false);
                }
            }
        }
    }

    public void CreateSelectionUnitTab() {
        if (unitCountPrev == 0 && SelectionManager.Instance.selectedObjects.Count == 0) return;
        if (SelectionManager.Instance.selectedObjects.Count == 1 && SelectionManager.Instance.IsBuilding(SelectionManager.Instance.selectedObjects[0])) return;

        unitCountPrev = SelectionManager.Instance.selectedObjects.Count;
        ClearTabs();
        unitSlotTabs.Clear();
        unitsAttachedToTab.Clear();
        unitQueue.Clear();

        Debug.Log("CreateSelectionUnitTab");
        foreach(var selectable in SelectionManager.Instance.selectedObjects) {
            if (selectable.selectableType == Selectable.SelectableType.Unit) {
                var unit = selectable.GetComponent<Unit>();
                var unitSo = unit.unitSo;

                if (unitQueue.ContainsKey(unitSo.unitName)) {
                    unitQueue[unitSo.unitName].Add(unitSo);
                } else {
                    unitQueue.Add(unitSo.unitName, new List<UnitSo> { unitSo });
                }
            }
        }

        foreach(var unit in unitQueue) {
            GameObject unitTab = Instantiate(unitSlotTabPrefab, transform);
            unitTab.name = unit.Key;
            var unitQueueCount = unit.Value.Count;

            SetUnitData(unitTab, unit.Value[0], unitQueueCount);
            unitSlotTabs.Add(unitTab);
            unitsAttachedToTab.Add(unit.Value[0]);
        }

        IsUnitUIOpen = true;
        IsUnitSelectionTabOpen = true;
    }

    private void SetUnitData(GameObject unitTab, UnitSo soUnit, int cost = -1) {
        var unitNameText = unitTab.GetComponentsInChildren<TextMeshProUGUI>()[0];
        var button = unitTab.GetComponentInChildren<Image>();

        unitNameText.text = soUnit.unitName;

        if (cost < 0 && soUnit.cost > 0) {
            var costText = unitTab.GetComponentsInChildren<TextMeshProUGUI>()[1];
            costText.text = soUnit.cost.ToString();
                
            button.GetComponent<Button>().onClick.AddListener(() => {
                spawnerBuilding.AddUnitToQueue(soUnit);
            });
        } else {
            var costText = unitTab.GetComponentsInChildren<TextMeshProUGUI>()[1];
            costText.text = cost.ToString();
        }

        Image[] images = unitTab.GetComponentsInChildren<Image>();
        var image = images[1];

        if (image is not null) {
            image.sprite = soUnit.sprite;
        }
    }

    public void ClearTabs() {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void CreateUnitTabs(BuildingSo BuildingSo, ISpawnerBuilding spawnerBuilding, GameObject building) {
        ClearTabs();
        unitSlotTabs.Clear();
        unitsAttachedToTab.Clear();
        selectedBuilding = BuildingSo;
        this.spawnerBuilding = spawnerBuilding;
        currentBuilding = building;
        // upgradeButton.gameObject.SetActive(true); 

        var currentTime = spawnerBuilding.GetSpawnTimer();
        var currentSpawningUnit = spawnerBuilding.GetCurrentSpawningUnit();

        foreach(var soUnit in BuildingSo.unitsToSpawn) {
            GameObject unitTab = Instantiate(unitSlotTabPrefab, transform);
            unitTab.name = soUnit.unitName;
            var unitQueueCount = spawnerBuilding.GetUnitQueueCountByName(soUnit.unitName);

            if (currentSpawningUnit is not null && currentSpawningUnit.unitName == soUnit.unitName) {
                var spawnPanel = unitTab.GetComponentInChildren<SpawnPanel>(true);
                spawnPanel.SetSpawnData(unitQueueCount, currentTime, spawnerBuilding.totalSpawnTime);
            } else if (unitQueueCount > 0) {
                var spawnPanel = unitTab.GetComponentInChildren<SpawnPanel>(true);
                spawnPanel.SetSpawnData(unitQueueCount, currentTime, spawnerBuilding.totalSpawnTime);
            }

            SetUnitData(unitTab, soUnit);
            unitSlotTabs.Add(unitTab);
            unitsAttachedToTab.Add(soUnit);
        }

        IsUnitUIOpen = true;
        IsUnitSelectionTabOpen = false;
    }
}
