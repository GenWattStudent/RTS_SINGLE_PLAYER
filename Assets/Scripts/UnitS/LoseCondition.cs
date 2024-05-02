using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LoseCondition : NetworkBehaviour
{
    private Damagable damagable;
    private Dictionary<int, int> teammateAlive = new();

    private void Start()
    {
        damagable = GetComponent<Damagable>();
        if (IsServer)
        {
            damagable.OnDead += OnDeadServerRpc;
            var playerControllers = NetworkManager.Singleton.ConnectedClients.Values;
            // Find player count for a team
            var teams = playerControllers.Select(x => x.PlayerObject.GetComponent<PlayerController>().playerData.teamId).Distinct();

            foreach (var team in teams)
            {
                teammateAlive.Add(team, playerControllers.Count(x => x.PlayerObject.GetComponent<PlayerController>().playerData.teamId == team));
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
    private void GameOverAllClientRpc(int winnerTeamId)
    {
        var playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        var gameResultObject = playerController.GetComponentInChildren<GameResult>();

        if (playerController.playerData.teamId == winnerTeamId)
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
        teammateAlive[playerController.playerData.teamId]--;

        if (teammateAlive[playerController.playerData.teamId] == 0)
        {
            var winnerTeamId = teammateAlive.FirstOrDefault(x => x.Value > 0);
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
