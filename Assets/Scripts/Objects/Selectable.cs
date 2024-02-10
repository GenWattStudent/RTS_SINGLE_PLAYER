using Unity.Netcode;
using UnityEngine;

// This class is used to identify objects that can be selected by the player. It can be building or units.
public class Selectable : NetworkBehaviour
{
    public enum SelectableType
    {
        Building,
        Unit
    }

    [SerializeField] private RectTransform selectionCircle;
    public SelectableType selectableType;
    [HideInInspector] public bool isSelected = false;
    private Damagable damagable;
    private Camera unitCamera;
    private SelectionManager selectionManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) enabled = false;
    }

    // when unit killed should be diselected
    private void Awake()
    {
        damagable = GetComponent<Damagable>();
        unitCamera = GetComponentInChildren<Camera>(true);
    }

    private void Start()
    {
        selectionManager = NetworkManager.LocalClient.PlayerObject.GetComponent<SelectionManager>();
    }

    private void OnDead()
    {
        if (!IsOwner) return;
        selectionManager.Deselect(this);
        Deselect();
    }

    public void Select()
    {
        if (!IsOwner) return;
        isSelected = true;
        damagable.OnDead += OnDead;
        if (selectionCircle == null) return;
        selectionCircle.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        if (!IsOwner) return;
        isSelected = false;
        if (unitCamera != null) unitCamera.gameObject.SetActive(false);
        damagable.OnDead -= OnDead;
        if (selectionCircle == null) return;
        selectionCircle.gameObject.SetActive(false);
    }
}
