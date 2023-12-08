using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITabManagement : MonoBehaviour
{
    private List<GameObject> tabs = new List<GameObject>();
    [SerializeField] private GameObject tabButtonPrefab;
    public string CurrentTab { get; private set; }

    private void CreateTabs()
    {
        // Make tabs from enum BuildingSo
        var unitTypes = System.Enum.GetValues(typeof(BuildingSo.BuildingType));

        foreach (var unitType in unitTypes)
        {
            // Create tab 
            GameObject tab = Instantiate(tabButtonPrefab, transform);
            tab.name = unitType.ToString();
            // Get text component from tab
            var tabText = tab.GetComponentInChildren<Text>();
            // Set text to tab
            tabText.text = unitType.ToString();
            // add event listener to tab
            tab.GetComponent<Button>().onClick.AddListener(() => {
                // Set current tab
                Debug.Log("Clicked on " + tab.name);
                CurrentTab = tab.name; 
                UIBuildingManager.Instance.CreateBuildingTabs((BuildingSo.BuildingType)System.Enum.Parse(typeof(BuildingSo.BuildingType), tab.name));
            });
            // Add tab to list
            tabs.Add(tab);
        }
    }

    void Start()
    {
        CreateTabs();
    }
}
