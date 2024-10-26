using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LoseCondition : NetworkBehaviour
{
    private Damagable damagable;
    private static Dictionary<TeamType, int> teammateAlive = new();

    private void Start()
    {
        if (IsServer)
        {
            damagable = GetComponent<Damagable>();
            damagable.OnDead += HandleDeath;
            CreateTeamDictionary();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            damagable.OnDead -= HandleDeath;
            CreateTeamDictionary();
        }
    }

    private void HandleDeath(Damagable damagable)
    {
        OnDeadServerRpc();
    }

    private void CreateTeamDictionary()
    {
        teammateAlive.Clear();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var connectedClientTeam = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>().teamType.Value;
            Debug.Log("ConnectedClientTeam " + connectedClientTeam);

            // Add team to dictionary
            if (!teammateAlive.ContainsKey(connectedClientTeam))
            {
                teammateAlive.Add(connectedClientTeam, 1);
            }
            else
            {
                teammateAlive[connectedClientTeam]++;
            }

            foreach (var team in teammateAlive)
            {
                Debug.Log("Team " + team.Key + " " + teammateAlive[team.Key]);
            }
        }
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams)
    {
        var playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        var gameResultObject = playerController.GetComponentInChildren<GameResult>();
        gameResultObject.Defeat();
    }

    [ClientRpc]
    private void GameOverAllClientRpc(TeamType winnerTeamId)
    {
        var playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        var gameResultObject = playerController.GetComponentInChildren<GameResult>();

        if (playerController.teamType.Value == winnerTeamId)
        {
            gameResultObject.Victory();
        }
        else
        {
            gameResultObject.Defeat();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnDeadServerRpc()
    {
        if (!IsServer) return;

        var playerController = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerController>();
        teammateAlive[playerController.teamType.Value]--;
        Debug.Log("OnDeadServerRpc " + teammateAlive[playerController.teamType.Value]);
        if (teammateAlive[playerController.teamType.Value] == 0)
        {
            var winnerTeamId = teammateAlive.FirstOrDefault(x => x.Value > 0);
            Debug.Log("Game Over " + winnerTeamId.Key);
            GameOverAllClientRpc(winnerTeamId.Key);
        }
        else
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            };

            GameOverClientRpc(clientRpcParams);
        }
    }

}
