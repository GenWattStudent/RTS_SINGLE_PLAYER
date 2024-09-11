using System.Collections.Generic;
using FOVMapping;
using Unity.Netcode;
using UnityEngine;

public class VisibilityManager : NetworkBehaviour
{
    private RTSObjectsManager rtsObjectsManager;
    private Dictionary<ulong, int> playerTeams = new Dictionary<ulong, int>();

    [SerializeField]
    private float visibilityUpdateInterval = 0.5f; // Update visibility every half second

    private float lastUpdateTime;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        rtsObjectsManager = GetComponent<RTSObjectsManager>();

        // Initialize player teams
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerController = client.PlayerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerTeams[client.ClientId] = playerController.playerData.teamId;
            }
        }

        // Set up visibility checks for all units
        foreach (var kvp in RTSObjectsManager.Units)
        {
            foreach (var unit in kvp.Value)
            {
                var networkObject = unit.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.CheckObjectVisibility += (ulong clientId) => CheckVisibility(clientId, networkObject, kvp.Key);
                }
            }
        }

        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        foreach (var kvp in RTSObjectsManager.Units)
        {
            foreach (var unit in kvp.Value)
            {
                if (unit == null) continue;
                var networkObject = unit.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.CheckObjectVisibility -= (ulong clientId) => CheckVisibility(clientId, networkObject, kvp.Key);
                }
            }
        }

        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (Time.time - lastUpdateTime >= visibilityUpdateInterval)
        {
            UpdateVisibility();
            lastUpdateTime = Time.time;
        }
    }

    private void UpdateVisibility()
    {
        foreach (var kvp in RTSObjectsManager.Units)
        {
            Debug.Log("Units: " + kvp.Key + " " + kvp.Value.Count);
        }

        foreach (var kvp in RTSObjectsManager.Units)
        {
            ulong ownerClientId = kvp.Key;
            List<Unit> units = kvp.Value;

            foreach (var unit in units)
            {
                var fovAgent = unit.GetComponent<FOVAgent>(); // Replace with actual FOV component type
                var networkObject = unit.GetComponent<NetworkObject>();

                if (fovAgent == null || networkObject == null) continue;

                foreach (var clientId in NetworkManager.ConnectedClientsIds)
                {
                    bool isServerClient = clientId == NetworkManager.ServerClientId;
                    if (isServerClient) continue;

                    // Ensure the client always sees its own units
                    if (clientId == ownerClientId)
                    {
                        if (!networkObject.IsNetworkVisibleTo(clientId))
                        {
                            networkObject.NetworkShow(clientId);
                        }
                        continue;
                    }

                    // Check visibility based on team and FOV
                    bool shouldBeVisible = CheckVisibility(clientId, networkObject, ownerClientId);
                    bool isVisible = networkObject.IsNetworkVisibleTo(clientId);

                    if (shouldBeVisible && !isVisible)
                    {
                        networkObject.NetworkShow(clientId);
                    }
                    else if (!shouldBeVisible && isVisible)
                    {
                        networkObject.NetworkHide(clientId);
                    }
                }
            }
        }
    }

    private bool CheckVisibility(ulong clientId, NetworkObject networkObject, ulong ownerClientId)
    {
        if (!networkObject.IsSpawned) return false;

        var fovAgent = networkObject.GetComponent<FOVAgent>();

        if (fovAgent == null) return false;

        // Check if the client is on the same team as the unit's owner
        bool sameTeam = playerTeams.TryGetValue(clientId, out int clientTeam) &&
                        playerTeams.TryGetValue(ownerClientId, out int unitTeam) &&
                        clientTeam == unitTeam;

        // If on the same team, always visible
        if (sameTeam) return true;

        Debug.Log("CheckVisibility: " + clientId + " " + ownerClientId + " " + fovAgent.IsUnderFOW());
        // If not on the same team, check FOV
        return !fovAgent.IsUnderFOW();
    }

    // Call this method when a new player joins or when a player's team changes
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerTeamServerRpc(ulong clientId, int teamId)
    {
        if (!IsServer) return;

        playerTeams[clientId] = teamId;
        UpdateVisibility(); // Immediately update visibility for all units
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (playerTeams.ContainsKey(clientId))
        {
            playerTeams.Remove(clientId);
        }
    }
}