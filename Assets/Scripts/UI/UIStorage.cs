using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Storage {
    public ResourceSO recourceSO;
    public int currentValue = 0;
    public StorageData storageData;
}

[System.Serializable]
public class StorageData {
    public ResourceSO resourceSO;
    public TextMeshProUGUI text;
    public RectTransform bar;
}

public class UIStorage : Singleton<UIStorage>
{
    [SerializeField] private List<StorageData> resources = new ();

    private List<Storage> storages = new ();

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
        storage.storageData.text.text = $"{storage.currentValue}/{storage.recourceSO.maxValue}";
        var progressBarScript = storage.storageData.bar.GetComponent<ProgresBar>();
        progressBarScript.UpdateProgresBar(storage.currentValue, storage.recourceSO.maxValue);
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

    private void Start() {
        CreateStorageForResources();

        foreach (var storage in storages) {
            UpdatResourceData(storage);
        }
    }
}
