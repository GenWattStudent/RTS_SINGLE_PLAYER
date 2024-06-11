using System.Collections.Generic;
using FOVMapping;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour
{
    public UnitSo unitSo;
    public AttackableSo attackableSo;
    public Material unitMaterial;
    public Material originalMaterial;
    public bool shouldChangeMaterial = true;
    public List<GameObject> unitPrefabs = new();
    public List<GameObject> unitUiPrefabs = new();
    public List<GameObject> bushes = new();
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

    public override void OnNetworkSpawn()
    {
        var playerColorData = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId];
        ChangeMaterial(playerColorData.playerMaterial, true);
    }

    public void HideUiPrefabs()
    {
        foreach (var unitUiPrefab in unitUiPrefabs)
        {
            unitUiPrefab.SetActive(false);
        }
    }

    public void ShowUiPrefabs()
    {
        foreach (var unitUiPrefab in unitUiPrefabs)
        {
            unitUiPrefab.SetActive(true);
        }
    }

    private void Start()
    {
        damagable = GetComponent<Damagable>();
        attack = GetComponent<Attack>();
        // visibleTimer = visibleInterval;

        var fovAgent = GetComponent<FOVAgent>();

        if (fovAgent == null)
        {
            fovAgent = gameObject.AddComponent<FOVAgent>();
        }

        fovAgent.disappearInFOW = !IsOwner;
        fovAgent.contributeToFOV = IsOwner;

        var fogOfWar = FindFirstObjectByType<FOVManager>();
        if (fogOfWar != null)
        {
            fogOfWar.AddFOVAgent(fovAgent);
        }


        if (damagable != null)
        {
            damagable.OnDead += HideUiPrefabs;
        }
    }

    public void HideUnit()
    {
        isVisibile = false;

        foreach (var unitPrefab in unitPrefabs)
        {
            var renderer = unitPrefab.GetComponent<Renderer>();
            renderer.enabled = false;
        }

        HideUiPrefabs();
    }

    public void ShowUnit()
    {
        isVisibile = true;

        foreach (var unitPrefab in unitPrefabs)
        {
            var renderer = unitPrefab.GetComponent<Renderer>();
            renderer.enabled = true;
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
