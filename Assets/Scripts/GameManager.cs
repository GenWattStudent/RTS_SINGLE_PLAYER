using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    [SerializeField] private bool isDebug = false;

    private void Start()
    {
        if (isDebug) return;

        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        if (RelayManager.Instance.IsHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
            RelayServerData relayServerData = new RelayServerData(RelayManager.Instance.Allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            RelayServerData relayServerData = new RelayServerData(RelayManager.Instance.JoinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;
    }
}