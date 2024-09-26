using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyData
{
    public string MapName = default;
    public string RelayCode = default;
    public Lobby CurrentLobby;

    public Dictionary<string, DataObject> Get()
    {
        Dictionary<string, DataObject> data = new Dictionary<string, DataObject>
        {
            { "MapName", new DataObject(DataObject.VisibilityOptions.Member, MapName) },
            { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, RelayCode) }
        };

        return data;
    }

    public async Task SetMapName(string mapName, string lobbyId)
    {
        MapName = mapName;
        Debug.Log("SetMapName: " + MapName);
        await UpdateLobbyData(lobbyId);
    }

    public async Task SetRelayCode(string relayCode, string lobbyId)
    {
        RelayCode = relayCode;

        await UpdateLobbyData(lobbyId);
    }

    public async Task UpdateLobbyData(string lobbyId)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions
        {
            Data = Get()
        };

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
    }

    public async Task GetLobbyData(string lobbyId)
    {
        CurrentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);

        if (CurrentLobby == null || CurrentLobby.Data == null) return;

        if (CurrentLobby.Data.ContainsKey("MapName"))
        {
            MapName = CurrentLobby.Data["MapName"].Value;
        }

        if (CurrentLobby.Data.ContainsKey("RelayCode"))
        {
            RelayCode = CurrentLobby.Data["RelayCode"].Value;
        }
    }
}
