using TMPro;
using UnityEngine;

public class Storage {
    public int energy = 200;
    public int maxEnergy = 700;
}

public class UIStorage : MonoBehaviour
{
    [SerializeField] private RectTransform energyBar;
    [SerializeField] private TextMeshProUGUI energyBarText;
    private Storage storage = new Storage();

    public static UIStorage Instance;

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

    public void decreaseEnergy(int amount) {
        if (storage.energy < amount) {
            return;
        }

        storage.energy -= amount;
        UpdateEnergyData();
    }

    private void Start() {
        Instance = this;
        UpdateEnergyData();
    }
}
