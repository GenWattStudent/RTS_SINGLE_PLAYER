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
        if (IsServer)
        {
            damagable.OnDead += OnDeadServerRpc;
            var playerControllers = NetworkManager.Singleton.ConnectedClients.Values;
            // Find player count for a team
            var teams = playerControllers.Select(x => x.PlayerObject.GetComponent<PlayerController>().teamType).Distinct();

            foreach (var team in teams)
            {
                teammateAlive.Add(team.Value, playerControllers.Count(x => x.PlayerObject.GetComponent<PlayerController>().teamType == team));
            }

            foreach (var teammate in teammateAlive)
            {
                Debug.Log(teammate.Key + " " + teammate.Value);
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
