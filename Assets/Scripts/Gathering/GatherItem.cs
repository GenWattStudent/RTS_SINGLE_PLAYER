using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GatherItem : NetworkBehaviour, IWorkerConstruction
{
    public GatherItemSo gatherItemSo;
    [HideInInspector] NetworkVariable<float> currentValue = new(0);
    [HideInInspector] NetworkVariable<bool> isGathered = new(false);
    [HideInInspector] NetworkVariable<float> maxValue = new(0);

    private List<Worker> workers = new();
    private ProgresBar progressBarScript;

    private void Awake()
    {
        progressBarScript = GetComponentInChildren<ProgresBar>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentValue.OnValueChanged += HandleCurrentValueChange;
        maxValue.OnValueChanged += HandleMaxValueChange;

        if (IsServer)
        {
            var randomValue = Random.Range(gatherItemSo.minValue, gatherItemSo.maxValue);
            currentValue.Value = randomValue;
            maxValue.Value = randomValue;
        }

        progressBarScript.UpdateProgresBar(currentValue.Value, maxValue.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        currentValue.OnValueChanged -= HandleCurrentValueChange;
        maxValue.OnValueChanged -= HandleMaxValueChange;
    }

    private void HandleCurrentValueChange(float oldValue, float newValue)
    {
        progressBarScript.UpdateProgresBar(newValue, maxValue.Value);
    }

    private void HandleMaxValueChange(float oldValue, float newValue)
    {
        progressBarScript.UpdateProgresBar(currentValue.Value, newValue);
    }

    public void AddWorker(Worker worker)
    {
        var damagable = worker.GetComponent<Damagable>();
        damagable.OnDead += HandleWorkerDeath;
        workers.Add(worker);
    }

    private void HandleWorkerDeath(Damagable damagable)
    {
        var worker = damagable.GetComponent<Worker>();
        workers.Remove(worker);
    }

    public void RemoveWorker(Worker worker)
    {
        var damagable = worker.GetComponent<Damagable>();
        damagable.OnDead -= HandleWorkerDeath;
        workers.Remove(worker);
        worker.StopConstructionServerRpc(false);
    }

    private void DestroyItem()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    private void Gather()
    {
        if (workers.Count == 0) return;

        var workersToRemove = new List<Worker>();

        foreach (var worker in workers)
        {
            var storage = NetworkManager.Singleton.ConnectedClients[worker.OwnerClientId].PlayerObject.GetComponentInChildren<UIStorage>();
            var stats = worker.GetComponent<Stats>();
            var gatherValue = stats.GetStat(StatType.Damage) * Time.deltaTime;

            if (storage.IsStorageFull(gatherItemSo.resourceSO))
            {
                workersToRemove.Add(worker);
                continue;
            }

            if (currentValue.Value <= 0)
            {
                isGathered.Value = true;
                DestroyItem();
                return;
            }

            if (currentValue.Value - gatherValue <= 0)
            {
                gatherValue = currentValue.Value;
            }

            if (storage.IsStorageFull(gatherItemSo.resourceSO, gatherValue))
            {
                gatherValue = storage.AmountCanFit(gatherItemSo.resourceSO, gatherValue);
            }

            currentValue.Value -= gatherValue;
            storage.IncreaseResource(gatherItemSo.resourceSO, gatherValue);
        }

        foreach (var worker in workersToRemove)
        {
            RemoveWorker(worker);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        Gather();
    }
}
