using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class LatencyManager : NetworkBehaviour
{
    public float Latency = 0f;

    [SerializeField] private float latencyUpdateInterval = 1f;
    private float _startTime;

    private void Start()
    {
        if (IsClient && IsLocalPlayer)
        {
            StartCoroutine(UpdateLatency());
        }
    }

    private IEnumerator UpdateLatency()
    {
        while (true)
        {
            yield return new WaitForSeconds(latencyUpdateInterval);

            if (IsClient)
            {
                _startTime = Time.time;
                RequestPingServerRpc();
            }
        }
    }

    [ServerRpc]
    private void RequestPingServerRpc(ServerRpcParams rpcParams = default)
    {
        RespondPingClientRpc(rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void RespondPingClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Latency = (Time.time - _startTime) * 1000f;
        }
    }
}
