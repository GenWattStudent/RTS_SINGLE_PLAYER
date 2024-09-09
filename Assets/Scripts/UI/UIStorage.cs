using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public struct Storage : IEquatable<Storage>, INetworkSerializable
{
    public int recourceIndex;
    public float currentValue;

    public Storage(int recourceIndex, float currentValue)
    {
        this.recourceIndex = recourceIndex;
        this.currentValue = currentValue;
    }

    public bool Equals(Storage other)
    {
        return recourceIndex == other.recourceIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref recourceIndex);
        serializer.SerializeValue(ref currentValue);
    }
}

[Serializable]
public class StorageData
{
    public ResourceSO resourceSO;
}

public class UIStorage : NetworkBehaviour
{
    [SerializeField] private List<StorageData> resources = new();
    private NetworkList<Storage> storages;
    private UIDocument UIDocument;
    private VisualElement root;

    private void Awake()
    {
        storages = new NetworkList<Storage>();
        storages.OnListChanged += OnStoragesChanged;
    }

    private void OnStoragesChanged(NetworkListEvent<Storage> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<Storage>.EventType.Add)
        {
            UpdateResourceData(changeEvent.Value);
        }
    }

    public Storage GetStorageByResource(ResourceSO resource)
    {
        var index = resources.FindIndex(x => x.resourceSO == resource);
        Storage storage = new Storage();

        for (int i = 0; i < storages.Count; i++)
        {
            if (storages[i].recourceIndex == index)
            {
                return storages[i];
            }
        }

        return storage;
    }

    private bool IsStorageFull(Storage storage)
    {
        var resource = resources[storage.recourceIndex];
        return storage.currentValue >= resource.resourceSO.maxValue;
    }

    private float AmountCanFit(Storage storage, float amount)
    {
        var resource = resources[storage.recourceIndex];
        if (storage.currentValue + amount > resource.resourceSO.maxValue)
        {
            return resource.resourceSO.maxValue - storage.currentValue;
        }

        if (storage.currentValue + amount < 0)
        {
            return -storage.currentValue;
        }

        return amount;
    }

    private void SendUpdateToClient(Storage storage)
    {
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = new ulong[] { OwnerClientId };
        var storageIndex = storages.IndexOf(storage);
        UpdateResourceDataClientRpc(storageIndex, clientRpcParams);
    }

    [ClientRpc]
    private void UpdateResourceDataClientRpc(int storageIndex, ClientRpcParams clientRpcParams = default)
    {
        var storage = storages[storageIndex];
        UpdateResourceData(storage);
    }

    public void IncreaseResource(ResourceSO resourceSO, float amount)
    {
        if (!IsServer) return;
        var storage = GetStorageByResource(resourceSO);
        var amountCanFit = AmountCanFit(storage, amount);

        storage.currentValue += amountCanFit;
        UpdateStorage(storage);
        SendUpdateToClient(storage);
    }

    private void UpdateStorage(Storage storage)
    {
        var index = storages.IndexOf(storage);
        storages[index] = storage;
    }

    public void DecreaseResource(ResourceSO resourceSO, float amount)
    {
        if (!IsServer) return;
        var storage = GetStorageByResource(resourceSO);
        Debug.Log($"Decreasing {resourceSO.resourceName} by {amount} {OwnerClientId}");
        var amountCanFit = AmountCanFit(storage, -amount);
        Debug.Log($"Amount can fit: {amountCanFit}");

        storage.currentValue += amountCanFit;
        Debug.Log($"New value: {storage.currentValue}");
        UpdateStorage(storage);
        SendUpdateToClient(storage);
    }

    public bool HasEnoughResource(ResourceSO resourceSO, float amount)
    {
        var storage = GetStorageByResource(resourceSO);

        return storage.currentValue >= amount;
    }

    [ClientRpc]
    private void CreateStorageClientRpc(ClientRpcParams clientRpcParams = default)
    {
        foreach (var storage in storages)
        {
            UpdateResourceData(storage);
        }
    }

    private void CreateStorageForResources()
    {
        foreach (var resource in resources)
        {
            var storage = new Storage(resources.IndexOf(resource), resource.resourceSO.startValue);

            storages.Add(storage);
        }

        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = new ulong[] { OwnerClientId };
        CreateStorageClientRpc(clientRpcParams);
    }

    private void OnEnable()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
    }

    private void UpdateResourceData(Storage storage)
    {
        var resource = resources[storage.recourceIndex];
        var progressBar = root.Q<ProgressBar>(resource.resourceSO.resourceName);

        if (progressBar is null) return;

        progressBar.lowValue = 0;
        progressBar.highValue = resource.resourceSO.maxValue;
        progressBar.value = storage.currentValue;
        progressBar.title = $"{resource.resourceSO.resourceName} {storage.currentValue}/{resource.resourceSO.maxValue}";
    }

    private void Start()
    {
        if (IsServer) CreateStorageForResources();
    }
}
