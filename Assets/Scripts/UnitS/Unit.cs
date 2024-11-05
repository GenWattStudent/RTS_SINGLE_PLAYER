using System;
using System.Collections.Generic;
using FOVMapping;
using RTS.Domain.SO;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour
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
    public NetworkVariable<bool> IsUpgrading = new(false);
    public bool isVisibile = true;

    private Damagable damagable;
    private PlayerController playerController;

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

    private void HandleTeamChange(TeamType oldValue, TeamType newValue)
    {
        AddAgentToFogOfWar(GetComponent<FOVAgent>(), newValue, damagable.teamType.Value);
    }

    private void HandleUnitTeamChange(TeamType oldValue, TeamType newValue)
    {
        AddAgentToFogOfWar(GetComponent<FOVAgent>(), playerController.teamType.Value, newValue);
    }

    private void AddAgentToFogOfWar(FOVAgent fovAgent, TeamType playerTeamType, TeamType unitTeamType)
    {
        var construction = GetComponent<Construction>();

        // if (unitTeamType != playerTeamType) HideUnit();
        // else ShowUnit();

        fovAgent.disappearInFOW = unitTeamType != playerTeamType;
        fovAgent.contributeToFOV = unitTeamType == playerTeamType && construction == null;

        var fogOfWar = FindFirstObjectByType<FOVManager>();
        if (fogOfWar != null && !fogOfWar.ContainsFOVAgent(fovAgent))
        {
            fogOfWar.AddFOVAgent(fovAgent);
        }
    }

    private void Start()
    {
        var playerColorData = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId];
        playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        ChangeMaterial(playerColorData.playerMaterial, true);

        damagable = GetComponent<Damagable>();

        playerController.teamType.OnValueChanged += HandleTeamChange;
        damagable.teamType.OnValueChanged += HandleUnitTeamChange;
        // visibleTimer = visibleInterval;

        var fovAgent = GetComponent<FOVAgent>();

        if (fovAgent == null)
        {
            fovAgent = gameObject.AddComponent<FOVAgent>();
        }

        AddAgentToFogOfWar(fovAgent, playerController.teamType.Value, damagable.teamType.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // NetworkManager.Singleton.NetworkTickSystem.Tick += OnNetworkTick;
        }

        if (!IsOwner) return;

        var rtsObjectManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RTSObjectsManager>();
        rtsObjectManager.AddLocalUnit(this);
    }

    // private void OnNetworkTick()
    // {
    //     UpdateVisibility();
    // }

    // private bool IsUnitInSight(Unit unit)
    // {
    //     var sightRange = unitSo.sightRange;
    //     var sightAngle = unitSo.sightAngle;

    //     var distance = Vector3.Distance(transform.position, unit.transform.position);
    //     if (distance > sightRange) return false;

    //     var directionToUnit = (unit.transform.position - transform.position).normalized;
    //     var angle = Vector3.Angle(transform.forward, directionToUnit);
    //     if (angle > sightAngle / 2) return false;

    //     return true;
    // }

    // private void UpdateVisibility()
    // {
    //     foreach (var player in RTSObjectsManager.Units)
    //     {
    //         // skip your team units
    //         var playerController = NetworkManager.Singleton.ConnectedClients[player.Key].PlayerObject.GetComponent<PlayerController>();
    //         var unitPlayerController = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();
    //         Debug.Log($"Player {playerController.OwnerClientId} have {player.Value.Count} units.");

    //         if (playerController.teamType.Value == unitPlayerController.teamType.Value) continue;

    //         foreach (var unit in player.Value)
    //         {
    //             if (unit == null) continue;

    //             if (IsUnitInSight(unit))
    //             {
    //                 Debug.Log($"Unit {unit.OwnerClientId} is in sight of {OwnerClientId}");
    //                 Show(unit);
    //             }
    //             else
    //             {
    //                 Debug.Log($"Unit {unit.OwnerClientId} is not in sight of {OwnerClientId}");
    //                 Hide(unit);
    //             }
    //         }
    //     }
    // }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            // NetworkManager.Singleton.NetworkTickSystem.Tick -= OnNetworkTick;
        }

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
