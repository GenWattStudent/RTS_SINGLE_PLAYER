using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyData : MonoBehaviour
{
    public string MapName = default;
    public string RelayCode = default;

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
}
