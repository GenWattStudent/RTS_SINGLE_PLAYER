using System.Collections.Generic;
using System.Linq;
using RTS.Domain.SO;
using RTS.UI;
using Unity.Netcode;
using UnityEngine;

namespace RTS.Managers
{
    public class UpgradeManager : NetworkBehaviour
    {
        public List<UpgradeSO> Upgrades;
        public UpgradeSO SelectedUpgrade;

        private SelectionManager _selectionManager;
        private UpgradeUI _upgradeUI;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _selectionManager = GetComponent<SelectionManager>();
            _upgradeUI = GetComponentInChildren<UpgradeUI>();
            _upgradeUI.OnUpgradeSelected += SelectUpgrade;
            SelectionManager.OnSelect += OnSelect;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            SelectionManager.OnSelect -= OnSelect;
            _upgradeUI.OnUpgradeSelected -= SelectUpgrade;
        }

        private void OnSelect()
        {
            if (_selectionManager.GetWorkers().Count > 0)
            {
                _upgradeUI.Show();
            }
            else
            {
                _upgradeUI.Hide();
            }
        }

        public void SelectUpgrade(UpgradeSO upgrade)
        {
            SelectedUpgrade = upgrade;
        }

        public bool CanApplyUpgrade(Unit unit)
        {
            return SelectedUpgrade != null && SelectedUpgrade.ForUnits.Any(u => unit.unitSo.name == u.name) && unit.Upgrades.All(u => u.name != SelectedUpgrade.name);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _upgradeUI.Hide();
                SelectedUpgrade = null;
            }
        }
    }
}
