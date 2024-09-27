using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LoseCondition : NetworkBehaviour
{
    private Damagable damagable;
    private Dictionary<TeamType, int> teammateAlive = new();

    private void Start()
    {
        damagable = GetComponent<Damagable>();

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        if (IsServer)
        {
            damagable.OnDead += OnDeadServerRpc;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            var playerControllers = NetworkManager.Singleton.ConnectedClients.Values;
            // Find player count for a team
            var teams = playerControllers.Select(x => x.PlayerObject.GetComponent<PlayerController>().teamType.Value).Distinct();
            teammateAlive.Clear();

            foreach (var team in teams)
            {
                if (teammateAlive.ContainsKey(team))
                {
                    teammateAlive[team]++;
                }
                else
                {
                    teammateAlive[team] = 1;
                }
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
