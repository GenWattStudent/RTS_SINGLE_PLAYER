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

        private UpgradeUI _upgradeUI;
        private UIStorage _uiStorage;
        private PlayerController _playerController;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _upgradeUI = GetComponentInChildren<UpgradeUI>();
            _uiStorage = GetComponentInChildren<UIStorage>();
            _playerController = GetComponent<PlayerController>();

            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _upgradeUI.OnUpgradeSelected += SelectUpgrade;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _upgradeUI.OnUpgradeSelected -= SelectUpgrade;
        }

        public void SelectUpgrade(UpgradeSO upgrade)
        {
            SelectedUpgrade = upgrade;
        }

        public bool CanApplyUpgrade(Unit unit, UpgradeSO upgrade)
        {
            return upgrade != null && unit != null && unit.unitSo != null &&
            upgrade.ForUnits.Any(u => u != null && unit.unitSo.unitName == u.unitName)
                   && unit.Upgrades.All(u => u != null && u.Name != upgrade.Name);
        }

        [ServerRpc]
        public void UpgradeServerRpc(NetworkObjectReference no, int index)
        {
            var upgrade = Upgrades[index];

            if (no.TryGet(out NetworkObject networkObject))
            {
                var unit = networkObject.GetComponent<Unit>();
                if (!CanApplyUpgrade(unit, upgrade) || !_uiStorage.HasEnoughResource(upgrade.costResource, upgrade.Cost)) return;

                unit.IsUpgrading.Value = true;
                unit.AddUpgrade(upgrade);

                var upgradeGo = Instantiate(upgrade.ConstructionManagerPrefab, unit.transform.position, Quaternion.identity);
                var upgradeNo = upgradeGo.GetComponent<NetworkObject>();
                var damagable = upgradeGo.GetComponent<Damagable>();
                var stats = upgradeGo.GetComponent<Stats>();
                var construction = upgradeGo.GetComponent<Construction>();

                construction.construction = upgrade;
                upgradeNo.SpawnWithOwnership(OwnerClientId);
                upgradeNo.transform.SetParent(unit.transform);
                damagable.teamType.Value = _playerController.teamType.Value;

                _uiStorage.DecreaseResource(upgrade.costResource, upgrade.Cost);
                stats.SetStat(StatType.Health, 1);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _upgradeUI.Hide();
                SelectedUpgrade = null;
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                if (_upgradeUI.isVisibile)
                {
                    _upgradeUI.Hide();
                }
                else
                {
                    _upgradeUI.Show();
                }
            }
        }
    }
}
