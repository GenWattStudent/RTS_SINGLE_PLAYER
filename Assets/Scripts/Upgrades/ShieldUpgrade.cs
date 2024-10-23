using RTS.Domain.SO;
using Unity.Netcode;
using UnityEngine;

public class ShieldUpgrade : NetworkBehaviour
{
    public float shieldPadding = 1f;
    public UpgradeSO upgradeSo;
    private Collider _collider;
    private Unit _unit;

    private void Start()
    {
        _collider = transform.parent.GetComponentInParent<Collider>();
        _unit = transform.parent.GetComponentInParent<Unit>();
        _collider.enabled = false;

        ScaleShield();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        _collider.enabled = true;
        _unit.RemoveUpgrade(upgradeSo);
    }

    private void ScaleShield()
    {
        // Scale shield to match the parent object
        var parent = transform.parent;
        var patentRenderer = parent.GetComponentInChildren<Renderer>();

        if (patentRenderer == null) return;

        var bounds = patentRenderer.bounds;
        var maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) + shieldPadding;
        transform.localScale = new Vector3(maxSize, maxSize, maxSize);

        // make sheld in the middle of y position
        var height = bounds.size.y;
        transform.position = new Vector3(transform.position.x, height / 2, transform.position.z);
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
