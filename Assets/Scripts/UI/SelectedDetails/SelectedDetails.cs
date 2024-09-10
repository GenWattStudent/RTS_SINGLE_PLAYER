using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Selectable;

public class SelectedDetails : NetworkToolkitHelper
{
    private Building building;
    private Button levelUpButton;
    private Button sellButton;
    private VisualElement statsContainer;
    private Label levelText;
    private ProgressBar healthBar;
    private ProgressBar expirenceBar;
    private VisualElement selectionInfo;
    private VisualElement actions;
    private bool isGoToTab = true;
    private SelectionManager selectionManager;
    private UIStorage uIStorage;
    private UITabManagement uITabManagement;
    private UnitDetailsUpdater unitDetailsUpdater;
    private BuildingDetailsUpdater buildingDetailsUpdater;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        InitializeUIElements();
        SetupManagers();

        unitDetailsUpdater = new UnitDetailsUpdater(statsContainer, healthBar, expirenceBar, levelText, actions);
        buildingDetailsUpdater = new BuildingDetailsUpdater(statsContainer, healthBar, expirenceBar, levelText, actions, uIStorage);

        levelUpButton.RegisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClick);

        ActivateButtons(false);
    }

    private void InitializeUIElements()
    {
        selectionInfo = root.Q<VisualElement>("SelectionInfo");
        levelUpButton = root.Q<Button>("LevelUp");
        sellButton = root.Q<Button>("Sell");
        statsContainer = root.Q<VisualElement>("Stats");
        levelText = root.Q<Label>("Level");
        healthBar = root.Q<ProgressBar>("Healthbar");
        expirenceBar = root.Q<ProgressBar>("Expirencebar");
        actions = GetVisualElement("Actions");
    }

    private void SetupManagers()
    {
        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
        var playerController = selectionManager.GetComponent<PlayerController>();
        uIStorage = playerController.GetComponentInChildren<UIStorage>();
        uITabManagement = playerController.GetComponentInChildren<UITabManagement>();
    }


    private void OnDisable()
    {

        // levelUpButton.UnregisterCallback<ClickEvent>(OnUpgradeButtonClick);
        // sellButton.UnregisterCallback<ClickEvent>(OnSellButtonClick);
    }

    private void ActivateButtons(bool isActive)
    {
        levelUpButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        sellButton.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnUpgradeButtonClick(ClickEvent ev)
    {
        if (building.buildingLevelable != null)
        {
            building.buildingLevelable.LevelUpServerRpc();
        }
    }

    private void OnSellButtonClick(ClickEvent ev)
    {
        Debug.Log("SellButtonClick " + building.buildingSo.buildingName + " " + building.buildingSo.cost);
        building.SellServerRpc();
    }

    private void CreateStat(string name, string value)
    {
        var statBox = new VisualElement
        {
            name = name
        };

        statBox.AddToClassList("stat");
        var statLabel = new Label(name);
        var statValue = new Label(value);

        statBox.Add(statLabel);
        statBox.Add(statValue);

        statsContainer.Add(statBox);
    }

    private void ClearStats()
    {
        List<VisualElement> elementsToRemove = new();

        foreach (var stat in statsContainer.Children())
        {
            elementsToRemove.Add(stat);
        }

        foreach (var stat in elementsToRemove)
        {
            statsContainer.Remove(stat);
        }
    }

    private void Hide()
    {
        selectionInfo.style.display = DisplayStyle.None;
    }

    private void Show()
    {
        selectionInfo.style.display = DisplayStyle.Flex;
    }

    private void UpdateMultipleDetails()
    {
        Show();
        actions.style.display = DisplayStyle.None;
        CreateStat("Selected", $"{selectionManager.selectedObjects.Count} units");
        ActivateButtons(false);
    }

    private void UpdateSelectedDetails()
    {
        ClearStats();

        if (selectionManager.selectedObjects.Count == 0)
        {
            Hide();
            if (!isGoToTab)
            {
                var tabs = System.Enum.GetValues(typeof(BuildingSo.BuildingType));
                var tabName = tabs.GetValue(0).ToString();
                uITabManagement.HandleTabClick(uITabManagement.GetTab(tabName));
                isGoToTab = true;
            }

            return;
        };

        isGoToTab = false;

        if (selectionManager.selectedObjects.Count == 1)
        {
            Show();
            var selectable = selectionManager.selectedObjects[0];
            var unit = selectable.GetComponent<Unit>();
            var stats = selectable.GetComponent<Stats>();

            if (unit != null && selectable.selectableType == SelectableType.Unit)
            {
                unitDetailsUpdater.UpdateUnitDetails(stats);
            }
            else
            {
                buildingDetailsUpdater.UpdateBuildingDetails(selectable);
            }
        }
        else
        {
            UpdateMultipleDetails();
        }
    }

    private void FixedUpdate()
    {
        UpdateSelectedDetails();
    }
}
