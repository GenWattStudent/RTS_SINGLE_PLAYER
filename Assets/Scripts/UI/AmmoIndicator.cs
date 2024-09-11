using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoIndicator : MonoBehaviour
{
    [SerializeField] private GameObject bulletIndicatorContainer;
    [SerializeField] private Image bulletIndicatorPrefab;
    private Attack attack;
    private List<Image> bulletIndicators = new();
    private Color initialColor;

    // Start is called before the first frame update
    private void Start()
    {
        attack = GetComponentInParent<Attack>();

        if (attack != null)
        {
            attack.OnAmmoChange += HandleAmmoChange;
            Debug.Log("attack.currentUnit.attackableSo.ammo: " + attack.currentUnit.attackableSo.ammo);
            DrawBulletIndicators(attack.currentUnit.attackableSo.ammo);
        }
    }

    private void DrawBulletIndicators(int maxAmmo)
    {
        for (int i = 0; i < maxAmmo; i++)
        {
            // create image and add this compnent to container
            var imagePanel = Instantiate(bulletIndicatorPrefab);
            var image = imagePanel.GetComponentsInChildren<Image>()[1];

            imagePanel.transform.SetParent(bulletIndicatorContainer.transform);
            imagePanel.transform.localScale = Vector3.one;
            imagePanel.transform.localPosition = new Vector3(imagePanel.transform.localPosition.x, imagePanel.transform.localPosition.y, 0);

            initialColor = image.color;
            bulletIndicators.Add(image);
        }
    }

    private void ChangeBulletIndicatorColor(bool active, int index = 0)
    {
        Debug.Log("ChangeBulletIndicatorColor: " + active + " index: " + index);
        bulletIndicators[index].color = active ? initialColor : Color.gray;
    }

    private void HandleAmmoChange(int currentAmmo)
    {
        Debug.Log("currentAmmo: " + currentAmmo);
        for (int i = 0; i < bulletIndicators.Count; i++)
        {
            if (i < currentAmmo)
            {
                ChangeBulletIndicatorColor(true, i);
            }
            else
            {
                ChangeBulletIndicatorColor(false, i);
            }
        }
    }
}
