using UnityEngine;

public class AmmoScreenController : MonoBehaviour
{
    private ScreenController screenController;
    private Attack attack;
    // Start is called before the first frame update
    void Start()
    {
        screenController = GetComponentInChildren<ScreenController>();
        attack = GetComponent<Attack>();
        screenController.SetProgresBar(0, attack.currentUnit.attackableSo.attackCooldown);
    }

    private void UpdateScreen() {
        screenController.SetText(attack.currentAmmo.ToString());
        screenController.SetProgresBar(attack.attackCooldownTimer, attack.currentUnit.attackableSo.attackCooldown);
    }

    // Update is called once per frame
    void Update()
    {
        if (screenController != null && attack != null) UpdateScreen();
    }
}
