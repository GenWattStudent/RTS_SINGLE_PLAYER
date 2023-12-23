using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    [SerializeField] private RectTransform statGameObject;
    private List<RectTransform> statObjects = new ();

    public void CreateStat(string name, string value) {
        var stat = Instantiate(statGameObject, transform);
        
        stat.name = name;
        var textMeshes = stat.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        textMeshes[0].text = name;
        textMeshes[1].text = value;
        statObjects.Add(stat);
    }

    public void ClearStats() {
        foreach (var statObject in statObjects) {
            Destroy(statObject.gameObject);
        }

        statObjects.Clear();
    }

    public void ClearStatByName(string name) {
        foreach (var statObject in statObjects) {
            if (statObject.name == name) {
                Destroy(statObject);
                statObjects.Remove(statObject);
                break;
            }
        }
    }
}
