using System.Collections.Generic;
using RTS.Domain.SO;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour, ISkillApplicable
{
    public UnitSo unitSo;
    public AttackableSo attackableSo;
    public Material unitMaterial;
    public Material originalMaterial;
    public List<GameObject> unitPrefabs = new();
    public List<GameObject> unitUiPrefabs = new();
    public List<GameObject> bushes = new();
    public List<UpgradeSO> Upgrades = new();
    public bool IsBot = false;
    public bool shouldChangeMaterial = true;
    public NetworkVariable<bool> isVisibile = new(true);
    public NetworkVariable<bool> IsUpgrading = new(false);

    public Damagable Damagable;
    public Stats Stats => GetComponent<Stats>();
    public string Name => unitSo.unitName;

    [ServerRpc(RequireOwnership = false)]
    public void CancelUpgradeServerRpc()
    {
        if (IsUpgrading.Value)
        {
            var construction = GetComponentInChildren<Construction>();
            construction.DestroyConstructionServerRpc();
            IsUpgrading.Value = false;
            // remove last upgrade
            Upgrades.RemoveAt(Upgrades.Count - 1);
        }
    }

    public void AddUpgrade(UpgradeSO upgrade)
    {
        Upgrades.Add(upgrade);
    }

    public void RemoveUpgrade(UpgradeSO upgrade)
    {
        // remove upgrade from list by name
        Upgrades.RemoveAll(u => u.Name == upgrade.Name);
    }

    public void ChangeMaterial(Material material, bool shouldChangeOriginalMaterial = false)
    {
        if (!shouldChangeMaterial) return;

        if (shouldChangeOriginalMaterial)
        {
            originalMaterial = material;
        }

        unitMaterial = material;

        if (unitMaterial != null)
        {
            foreach (var unitPrefab in unitPrefabs)
            {
                var renderer = unitPrefab.GetComponent<Renderer>();
                renderer.material = unitMaterial;
            }
        }
    }

    public void HideUiPrefabs(Unit unit)
    {
        var canvases = unit.GetComponentsInChildren<Canvas>();
        foreach (var canvas in canvases)
        {
            canvas.enabled = false;
        }
    }

    public void ShowUiPrefabs(Unit unit)
    {
        var canvases = unit.GetComponentsInChildren<Canvas>();
        foreach (var canvas in canvases)
        {
            canvas.enabled = true;
        }
    }

    private void Start()
    {
        var playerColorData = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId];

        ChangeMaterial(playerColorData.playerMaterial, true);

        Damagable = GetComponent<Damagable>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        var rtsObjectManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RTSObjectsManager>();
        rtsObjectManager.AddLocalUnit(this);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        var rtsObjectManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RTSObjectsManager>();
        rtsObjectManager.RemoveLocalUnit(this);
    }

    public void HideUnit(Unit unit)
    {
        // get all renderers in children and from compoenent and disable them
        var renderers = unit.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // ligths 
        var lights = unit.GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
        {
            light.enabled = false;
        }

        // line renderers
        var lineRenderers = unit.GetComponentsInChildren<LineRenderer>(true);
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = false;
        }

        HideUiPrefabs(unit);
    }

    public void ShowUnit(Unit unit)
    {
        // get all renderers in children and from compoenent and enable them
        var renderers = unit.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        // ligths
        var lights = unit.GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
        {
            light.enabled = true;
        }

        // line renderers
        var lineRenderers = unit.GetComponentsInChildren<LineRenderer>(true);
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = true;
        }

        ShowUiPrefabs(unit);
    }
}
