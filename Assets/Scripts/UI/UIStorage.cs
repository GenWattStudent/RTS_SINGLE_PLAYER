using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Storage {
    public ResourceSO recourceSO;
    public int currentValue = 0;
    public StorageData storageData;
}

[System.Serializable]
public class StorageData {
    public ResourceSO resourceSO;
}

public class UIStorage : Singleton<UIStorage>
{
    [SerializeField] private List<StorageData> resources = new ();
    private List<Storage> storages = new ();
    private UIDocument UIDocument;
    private VisualElement root;

    public Storage GetStorageByResource(ResourceSO resource) {
        return storages.Find(x => x.recourceSO == resource);
    }

    private bool IsStorageFull(Storage storage) {
        return storage.currentValue >= storage.recourceSO.maxValue;
    }

    private int AmountCanFit(Storage storage, int amount) {
        if (storage.currentValue + amount > storage.recourceSO.maxValue) {
            return storage.recourceSO.maxValue - storage.currentValue;
        }

        if (storage.currentValue + amount < 0) {
            return 0;
        }

        return amount;
    }

    private void UpdatResourceData(Storage storage) {
        var progressBar = root.Q<ProgressBar>(storage.recourceSO.resourceName);

        if (progressBar is null) return;
        
        progressBar.lowValue = 0;
        progressBar.highValue = storage.recourceSO.maxValue;
        progressBar.value = storage.currentValue;
        progressBar.title = $"{storage.recourceSO.resourceName} {storage.currentValue}/{storage.recourceSO.maxValue}";
    }

    public void IncreaseResource(ResourceSO resourceSO, int amount) {
        var storage = GetStorageByResource(resourceSO);

        if (storage is null) {
            return;
        }

        var amountCanFit = AmountCanFit(storage, amount);

        storage.currentValue += amountCanFit;
        UpdatResourceData(storage);
    }

    public void DecreaseResource(ResourceSO resourceSO, int amount) {
        var storage = GetStorageByResource(resourceSO);

        if (storage is null) {
            return;
        }

        var amountCanFit = AmountCanFit(storage, -amount);

        storage.currentValue += amountCanFit;
        UpdatResourceData(storage); 
    }

    public bool HasEnoughResource(ResourceSO resourceSO, int amount) {
        var storage = GetStorageByResource(resourceSO);

        if (storage is null) {
            return false;
        }

        return storage.currentValue >= amount;
    }

    private void CreateStorageForResources() {
        foreach (var resource in resources) {
            var storage = new Storage
            {
                recourceSO = resource.resourceSO,
                currentValue = resource.resourceSO.startValue,
                storageData = resource
            };

            storages.Add(storage);
        }
    }

    private void OnEnable() {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
    }

    private void Start() {
        CreateStorageForResources();

        foreach (var storage in storages) {
            UpdatResourceData(storage);
        }
    }
}
