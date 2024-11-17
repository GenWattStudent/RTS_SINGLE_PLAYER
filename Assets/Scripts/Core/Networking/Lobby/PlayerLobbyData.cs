using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public enum TeamType
{
    None,
    Red,
    Blue
}

public class PlayerLobbyData
{
    public string PlayerId = default;
    public string PlayerName = default;
    public TeamType Team = TeamType.None;
    public bool IsReady = false;

    public string AllocationId = default;
    public string ConnectionData = default;

    public PlayerLobbyData(string playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public PlayerLobbyData(string playerId, string playerName, TeamType team)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        Team = team;
    }

    public PlayerLobbyData(Player player)
    {
        PlayerId = player.Id;

        if (player.Data != null && player.Data.ContainsKey("PlayerName"))
        {
            PlayerName = player.Data["PlayerName"].Value;
        }

        if (player.Data != null && player.Data.ContainsKey("Team"))
        {
            Team = (TeamType)System.Enum.Parse(typeof(TeamType), player.Data["Team"].Value);
        }

        if (player.Data != null && player.Data.ContainsKey("IsReady"))
        {
            IsReady = player.Data["IsReady"].Value == "True";
        }
    }

    public async Task SetTeam(TeamType team, string lobbyId)
    {
        Team = team;

        await UpdatePlayerData(lobbyId);
    }

    public async Task SetReady(bool isReady, string lobbyId)
    {
        IsReady = isReady;

        await UpdatePlayerData(lobbyId);
    }

    public async Task SetConnectionData(string allocationId, string connectionData, string lobbyId)
    {
        AllocationId = allocationId;
        ConnectionData = connectionData;

        await UpdatePlayerData(lobbyId);
    }

    public Dictionary<string, PlayerDataObject> Get()
    {
        Dictionary<string, PlayerDataObject> playerData = new()
        {
            { "Team", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Team.ToString()) },
            { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, IsReady.ToString()) },
            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName) }
        };

        return playerData;
    }

    public async Task UpdatePlayerData(string lobbyId)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = Get(),
            AllocationId = AllocationId,
            ConnectionInfo = ConnectionData
        };

        await LobbyService.Instance.UpdatePlayerAsync(lobbyId, PlayerId, options);
    }
}
