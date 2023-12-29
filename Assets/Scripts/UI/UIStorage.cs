using TMPro;
using UnityEngine;

public class Storage {
    public int energy = 200;
    public int maxEnergy = 700;
    public int mass = 800;
    public int maxMass = 5000;
}

public class UIStorage : Singleton<UIStorage>
{
    [SerializeField] private RectTransform energyBar;
    [SerializeField] private TextMeshProUGUI energyBarText;

    [SerializeField] private RectTransform massBar;
    [SerializeField] private TextMeshProUGUI massBarText;

    private Storage storage = new Storage();

    #region Energy

    private void UpdateEnergyData() {
        energyBarText.text = $"{storage.energy}/{storage.maxEnergy}";
        var progressBarScript = energyBar.GetComponent<ProgresBar>();
        progressBarScript.UpdateProgresBar(storage.energy, storage.maxEnergy);
    }

    public void IncreaseEnergy(int amount) {
        if (storage.energy + amount > storage.maxEnergy) {
            return;
        }
        storage.energy += amount;
        UpdateEnergyData();
    }

    public void DecreaseEnergy(int amount) {
        if (storage.energy < amount) {
            return;
        }

        storage.energy -= amount;
        UpdateEnergyData();
    }

    public bool HasEnoughEnergy(int amount) {
        return storage.energy >= amount;
    }

    #endregion

    #region Mass

    private void UpdateMassData() {
        massBarText.text = $"{storage.mass}/{storage.maxMass}";
        var progressBarScript = massBar.GetComponent<ProgresBar>();
        progressBarScript.UpdateProgresBar(storage.mass, storage.maxMass);
    }

    public void IncreaseMass(int amount) {
        if (storage.mass + amount > storage.maxMass) {
            return;
        }
        storage.mass += amount;
        UpdateMassData();
    }

    public void DecreaseMass(int amount) {
        if (storage.mass < amount) {
            return;
        }

        storage.mass -= amount;
        UpdateMassData();
    }

    public bool HasEnoughMass(int amount) {
        return storage.mass >= amount;
    }

    #endregion

    private void Start() {
        UpdateEnergyData();
        UpdateMassData();
    }
}
