using UnityEngine;
using UnityEngine.UI;

public class ProgresBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void UpdateProgresBar(float currentHealth, float maxHealth) {
        slider.value = currentHealth / maxHealth;
    }

    void Update()
    {
        
    }
}
