using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Selectable;

public class SelectedDetails : NetworkToolkitHelper
{
    private Selectable selectedObject;
    private SelectionManager selectionManager;
    private UIStorage uIStorage;
    private UITabManagement uITabManagement;
    private UnitDetailsUpdater unitDetailsUpdater;
    private BuildingDetailsUpdater buildingDetailsUpdater;
    private bool isGoToTab = true;
    // UI Elements
    private Button levelUpButton;
    private Button sellButton;
    private Button cancelUpgradeButton;
    private VisualElement statsContainer;
    private Label levelText;
    private ProgressBar healthBar;
    private ProgressBar expirenceBar;
    private VisualElement selectionInfo;
    private VisualElement actions;
    private VisualElement attackActions;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        InitializeUIElements();
        SetupManagers();

        unitDetailsUpdater = new UnitDetailsUpdater(
            statsContainer,
            healthBar,
            expirenceBar,
            levelText,
            actions,
            levelUpButton,
            sellButton,
            attackActions,
            cancelUpgradeButton);

        buildingDetailsUpdater = new BuildingDetailsUpdater(
            statsContainer,
            healthBar,
            expirenceBar,
            levelText,
            actions,
            uIStorage,
            levelUpButton,
            sellButton,
            attackActions);

        levelUpButton.RegisterCallback<ClickEvent>(OnUpgradeButtonClick);
        sellButton.RegisterCallback<ClickEvent>(OnSellButtonClick);
        cancelUpgradeButton.RegisterCallback<ClickEvent>(OnCancelUpgradeButtonClick);

        SelectionManager.OnSelect += UpdateSelectedDetails;

        ActivateButtons(false);
        UpdateSelectedDetails();
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
        cancelUpgradeButton = root.Q<Button>("CancelUpgrade");
        actions = GetVisualElement("Actions");
        attackActions = GetVisualElement("AttackActions");
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
        var building = selectedObject.GetComponent<Building>();
        if (building != null && building.buildingLevelable != null)
        {
            building.buildingLevelable.LevelUpServerRpc();
        }
    }

    private void OnCancelUpgradeButtonClick(ClickEvent ev)
    {
        var unit = selectedObject.GetComponent<Unit>();
        if (unit != null) unit.CancelUpgradeServerRpc();
    }

    private void OnSellButtonClick(ClickEvent ev)
    {
        var building = selectedObject.GetComponent<Building>();
        if (building != null) building.SellServerRpc();
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
        selectedObject = null;

        if (selectionManager.selectedObjects.Count == 0)
        {
            if (!isGoToTab)
            {
                var tabs = System.Enum.GetValues(typeof(BuildingSo.BuildingType));
                var tabName = tabs.GetValue(0).ToString();
                uITabManagement.HandleTabClick(uITabManagement.GetTab(tabName));
                isGoToTab = true;
            }

            Hide();
            return;
        };

        isGoToTab = false;

        if (selectionManager.selectedObjects.Count == 1)
        {
            var selectable = selectionManager.selectedObjects[0];
            selectedObject = selectable;
            Show();

            var stats = selectable.GetComponent<Stats>();

            if (selectable.selectableType == SelectableType.Unit)
            {
                actions.style.display = DisplayStyle.None;
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

    // private void FixedUpdate()
    // {
    //     UpdateSelectedDetails();
    // }
}
