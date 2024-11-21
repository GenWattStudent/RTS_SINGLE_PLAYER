using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PowerUpItem : NetworkBehaviour
{
    public NetworkVariable<bool> IsRespawning = new(false);
    public NetworkVariable<float> RespawnTimer = new(0);

    [SerializeField] private PowerUpSo powerUpSo;
    private NetworkObject _powerUpNetworkObject;
    private float _spawnInterval;
    private TextMeshProUGUI _timerText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            _timerText = GetComponentInChildren<TextMeshProUGUI>();
            ChangeTextVisibility(IsRespawning.Value);
            RespawnTimer.OnValueChanged += OnRespawnTimerChanged;
            IsRespawning.OnValueChanged += OnRespawningChanged;
        }

        if (IsServer)
        {
            _spawnInterval = powerUpSo.Cooldown;

            Spawn();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsClient)
        {
            RespawnTimer.OnValueChanged -= OnRespawnTimerChanged;
            IsRespawning.OnValueChanged -= OnRespawningChanged;
        }
    }

    private void OnRespawningChanged(bool previousValue, bool newValue)
    {
        ChangeTextVisibility(newValue);
    }

    private void ChangeTextVisibility(bool value)
    {
        _timerText.gameObject.SetActive(value);
    }

    private void OnRespawnTimerChanged(float previousValue, float newValue)
    {
        _timerText.text = Math.Ceiling(newValue).ToString();
    }

    private void Spawn()
    {
        var go = Instantiate(powerUpSo.Prefab, transform.position, Quaternion.identity);

        _powerUpNetworkObject = go.GetComponent<NetworkObject>();
        _powerUpNetworkObject.Spawn();
        _powerUpNetworkObject.transform.SetParent(transform);

        IsRespawning.Value = false;
    }

    private void Update()
    {
        if (IsRespawning.Value) return;

        Move();
    }

    private void Move()
    {
        transform.position = new Vector3(transform.position.x, Mathf.PingPong(Time.time / 2, 1) + 0.35f, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || IsRespawning.Value) return;

        if (other.TryGetComponent(out Stats stats))
        {
            HandlePickup(stats);
        }
    }

    private void HandlePickup(Stats stats)
    {
        stats.AddPowerUp(powerUpSo);

        _powerUpNetworkObject.Despawn(true);
        _powerUpNetworkObject = null;

        if (powerUpSo.IsRespawnable)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        IsRespawning.Value = true;
        RespawnTimer.Value = _spawnInterval;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        while (RespawnTimer.Value > 0)
        {
            RespawnTimer.Value -= 1;
            yield return new WaitForSeconds(1f);
        }

        Spawn();
    }
}
