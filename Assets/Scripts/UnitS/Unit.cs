using System.Collections.Generic;
using FOVMapping;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public UnitSo unitSo;
    public bool IsBot = false;
    public AttackableSo attackableSo;
    public Material unitMaterial;
    public Material originalMaterial;
    public bool shouldChangeMaterial = true;
    public List<GameObject> unitPrefabs = new();
    public List<GameObject> unitUiPrefabs = new();
    public List<GameObject> bushes = new();
    public TeamType teamType;
    private Damagable damagable;
    private Attack attack;
    public bool isVisibile = true;
    // private float visibleTimer = 0f;
    private float visibleInterval = 5f;


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

    // public override void OnNetworkSpawn()
    // {
    //     if (!IsOwner) HideUnit();
    //     var id = IsBot ? 2 : OwnerClientId;
    //     var playerColorData = MultiplayerController.Instance.playerMaterials[(int)id];

    //     ChangeMaterial(playerColorData.playerMaterial, true);
    // }

    public void HideUiPrefabs()
    {
        var canvases = GetComponentsInChildren<Canvas>();
        foreach (var canvas in canvases)
        {
            canvas.enabled = false;
        }
    }

    public void ShowUiPrefabs()
    {
        var canvases = GetComponentsInChildren<Canvas>();
        foreach (var canvas in canvases)
        {
            canvas.enabled = true;
        }
    }

    private void Start()
    {

        var playerColorData = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId];
        var playerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();

        ChangeMaterial(playerColorData.playerMaterial, true);

        damagable = GetComponent<Damagable>();
        attack = GetComponent<Attack>();
        // visibleTimer = visibleInterval;
        if (damagable.teamType.Value != playerController.teamType.Value) HideUnit();
        var fovAgent = GetComponent<FOVAgent>();

        if (fovAgent == null)
        {
            fovAgent = gameObject.AddComponent<FOVAgent>();
        }

        var construction = GetComponent<Construction>();

        fovAgent.disappearInFOW = damagable.teamType.Value != playerController.teamType.Value;
        fovAgent.contributeToFOV = damagable.teamType.Value == playerController.teamType.Value && construction == null;

        var fogOfWar = FindFirstObjectByType<FOVManager>();
        if (fogOfWar != null)
        {
            fogOfWar.AddFOVAgent(fovAgent);
        }
    }

    public void HideUnit()
    {
        isVisibile = false;

        // get all renderers in children and from compoenent and disable them
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // ligths 
        var lights = GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
        {
            light.enabled = false;
        }

        // line renderers
        var lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = false;
        }

        HideUiPrefabs();
    }

    public void ShowUnit()
    {
        isVisibile = true;

        // get all renderers in children and from compoenent and enable them
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        // ligths
        var lights = GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
        {
            light.enabled = true;
        }

        // line renderers
        var lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = true;
        }

        ShowUiPrefabs();
    }

    private void Update()
    {
        // visibleTimer += Time.deltaTime;

        // if (attack != null && bushes.Count > 0 && attack.targetPosition != Vector3.zero)
        // {
        //     ShowUnit();
        //     // visibleTimer = 0f;
        //     return;
        // }

        // if (bushes.Count > 0)
        // {
        //     HideUnit();
        // }
        // else
        // {
        //     ShowUnit();
        // }
    }
}
