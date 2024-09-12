using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AmmoIndicatorType
{
    Active,
    InActive,
    Reload
}

public class AmmoIndicator : MonoBehaviour
{
    [SerializeField] private GameObject bulletIndicatorContainer;
    [SerializeField] private Image bulletIndicatorPrefab;
    private Attack attack;
    private List<Image> bulletIndicators = new();
    private Color initialColor;
    private Color realoadColor = new Color(255, 203, 59, 1);
    private Coroutine fillBulletIndicatorsCoroutine;
    private float initialReloadDelay = 0.5f;

    // Start is called before the first frame update
    private void Start()
    {
        attack = GetComponentInParent<Attack>();

        if (attack != null)
        {
            attack.OnAmmoChange += HandleAmmoChange;
            DrawBulletIndicators(attack.currentUnit.attackableSo.ammo);
        }
    }

    private void OnDestroy()
    {
        if (attack != null)
        {
            attack.OnAmmoChange -= HandleAmmoChange;
            StopFillBulletIndicators();
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

    private void Reload()
    {
        // realod animation calulate time per 1 bullet and start filling bullet indicators
        var timePerBullet = (attack.currentUnit.attackableSo.attackCooldown - initialReloadDelay - .4f) / attack.currentUnit.attackableSo.ammo;

        // start filling bullet indicators
        fillBulletIndicatorsCoroutine = StartCoroutine(FillBulletIndicators(timePerBullet));
    }

    // courutine for filling bullet indicators
    private IEnumerator FillBulletIndicators(float time)
    {
        yield return new WaitForSeconds(initialReloadDelay);
        for (int i = 0; i < bulletIndicators.Count; i++)
        {
            ChangeBulletIndicatorColor(AmmoIndicatorType.Reload, i);
            yield return new WaitForSecondsRealtime(time);
        }

        StopFillBulletIndicators();
    }

    private void StopFillBulletIndicators()
    {
        if (fillBulletIndicatorsCoroutine != null)
        {
            StopCoroutine(fillBulletIndicatorsCoroutine);
            fillBulletIndicatorsCoroutine = null;
        }
    }

    private void ChangeBulletIndicatorColor(AmmoIndicatorType type, int index = 0)
    {
        switch (type)
        {
            case AmmoIndicatorType.Active:
                bulletIndicators[index].color = initialColor;
                break;
            case AmmoIndicatorType.InActive:
                bulletIndicators[index].color = Color.gray;
                break;
            case AmmoIndicatorType.Reload:
                bulletIndicators[index].color = realoadColor;
                break;
        }
    }

    private void HandleAmmoChange(int currentAmmo)
    {
        for (int i = 0; i < bulletIndicators.Count; i++)
        {
            if (i < currentAmmo)
            {
                ChangeBulletIndicatorColor(AmmoIndicatorType.Active, i);
            }
            else
            {
                ChangeBulletIndicatorColor(AmmoIndicatorType.InActive, i);
            }
        }

        if (currentAmmo == 0)
        {
            Reload();
        }

        if (currentAmmo > 0)
        {
            StopFillBulletIndicators();
        }
    }
}
