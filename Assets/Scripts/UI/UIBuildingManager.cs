using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingManager : MonoBehaviour
{
    [SerializeField] private BuildingSo[] buildings;
    [SerializeField] private GameObject buildingTabPrefab;
    // [SerializeField] private Button upgradeButton;
    private BuildingSo selectedBuilding;
    public static UIBuildingManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ClearTabs() {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public BuildingSo GetSelectedBuilding() {
        return selectedBuilding;
    }

    public void SetSelectedBuilding(BuildingSo building) {
        selectedBuilding = building;
    }

    public void CreateBuildingTabs(BuildingSo.BuildingType buildingType) {
        ClearTabs();
        UIUnitManager.Instance.IsUnitUIOpen = false;
        // upgradeButton.gameObject.SetActive(false);

        foreach (var bulding in buildings)
        {
            if (bulding.type == buildingType)
            {
                CreateBuildingTab(bulding);
            }
        }
    }

    private void SetBuildingData(GameObject buildingTab, BuildingSo BuildingSo) {
        var buildingNameText = buildingTab.GetComponentsInChildren<TextMeshProUGUI>()[0];
        var costText = buildingTab.GetComponentsInChildren<TextMeshProUGUI>()[1];
        var button = buildingTab.GetComponentInChildren<Image>();

        buildingNameText.text = BuildingSo.buildingName;
        costText.text = BuildingSo.cost.ToString();

        button.GetComponent<Button>().onClick.AddListener(() => {
            Debug.Log("Clicked on " + BuildingSo.buildingName);
            selectedBuilding = BuildingSo;
        });

        Image[] images = buildingTab.GetComponentsInChildren<Image>();

        var image = images[1];

        if (image is not null) {
            image.sprite = BuildingSo.sprite;
        }
    }

    public void CreateBuildingTab(BuildingSo bulding) {
        GameObject buildingTab = Instantiate(buildingTabPrefab, transform);
        buildingTab.name = bulding.buildingName;

        SetBuildingData(buildingTab, bulding);
    }
}
