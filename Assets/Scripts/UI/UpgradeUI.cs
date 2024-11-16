using System;
using System.Collections.Generic;
using RTS.Domain.SO;
using RTS.Managers;
using UnityEngine;
using UnityEngine.UIElements;

namespace RTS.UI
{
    public class UpgradeUI : NetworkToolkitHelper
    {
        public bool isVisibile => _upgradeItemContainer.style.display == DisplayStyle.Flex;
        [SerializeField] private VisualTreeAsset _upgradeItemTemplate;

        private ScrollView _upgradeItemContainer;
        private List<VisualElement> _upgradeItems = new();
        private UpgradeManager _upgradeManager;
        private UIStorage _uiStorage;
        private PlayerController _playerController;

        public event Action<UpgradeSO> OnUpgradeSelected;

        protected override void OnEnable()
        {
            base.OnEnable();
            _upgradeItemContainer = root.Q<ScrollView>("UpgradeContainer");

            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

        private void Start()
        {
            _upgradeManager = GetComponentInParent<UpgradeManager>();
            _uiStorage = GetComponent<UIStorage>();
            _playerController = GetComponentInParent<PlayerController>();
        }

        public void Hide()
        {
            _upgradeItemContainer.Clear();
            _upgradeItemContainer.style.display = DisplayStyle.None;
        }

        public void Show()
        {
            _upgradeItemContainer.style.display = DisplayStyle.Flex;
            CreateList();
        }

        private void CreateList()
        {
            _upgradeItemContainer.Clear();
            _upgradeItems.Clear();

            for (int i = 0; i < _upgradeManager.Upgrades.Count; i++)
            {
                CreateItem(_upgradeManager.Upgrades[i], i);
            }
        }

        private void SelectUpgrade(UpgradeSO upgrade, VisualElement upgradeItem)
        {
            foreach (var item in _upgradeItems)
            {
                item.RemoveFromClassList("active");
            }

            upgradeItem.AddToClassList("active");
            OnUpgradeSelected?.Invoke(upgrade);
        }

        private void CreateItem(UpgradeSO upgrade, int index)
        {
            var upgradeItem = _upgradeItemTemplate.CloneTree();
            upgradeItem.Q<VisualElement>("UpgradeItem").style.backgroundImage = new StyleBackground(upgrade.Icon);
            upgradeItem.Q<VisualElement>("UpgradeItem").RegisterCallback((ClickEvent evt) => SelectUpgrade(upgrade, upgradeItem));

            if (_upgradeManager.Upgrades.Count != 1 || index != _upgradeManager.Upgrades.Count - 1)
            {
                upgradeItem.Q<VisualElement>("UpgradeItem").style.marginBottom = 8;
            }

            if (!_uiStorage.HasEnoughResource(upgrade.costResource, upgrade.Cost) || upgrade.UnlockLevel > _playerController.playerLevel.Value)
            {
                Debug.Log($"Not enough resources or unlock level {_playerController.playerLevel.Value} {_uiStorage.HasEnoughResource(upgrade.costResource, upgrade.Cost)}");
                upgradeItem.Q<VisualElement>("UpgradeItem").SetEnabled(false);
            }

            _upgradeItemContainer.Add(upgradeItem);
            _upgradeItems.Add(upgradeItem);
        }
    }
}