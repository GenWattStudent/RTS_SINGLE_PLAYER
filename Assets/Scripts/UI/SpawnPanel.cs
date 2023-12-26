using TMPro;
using UnityEngine;

public class SpawnPanel : MonoBehaviour
{
    public void SetSpawnData(int unitQueueCount, float currentTime, float totalSpawnTime) {
        gameObject.SetActive(true);

        var spawnUnitCountText = GetComponentsInChildren<TextMeshProUGUI>(true)[0];
        var timeText = GetComponentsInChildren<TextMeshProUGUI>(true)[1];
        var progressBar = GetComponentInChildren<ProgresBar>(true);
        var timeRounded = Mathf.RoundToInt(currentTime);

        timeText.text = timeRounded.ToString() + "s";
        spawnUnitCountText.text = unitQueueCount.ToString() + "x";
        progressBar.UpdateProgresBar(currentTime, totalSpawnTime);
    }
}
