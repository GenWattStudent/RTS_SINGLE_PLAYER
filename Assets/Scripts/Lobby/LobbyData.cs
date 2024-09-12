using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyData : MonoBehaviour
{
    public string MapName;
    public string RelayCode;

    public Dictionary<string, DataObject> Get()
    {
        Dictionary<string, DataObject> data = new Dictionary<string, DataObject>
        {
            { "MapName", new DataObject(DataObject.VisibilityOptions.Member, MapName) },
            { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, RelayCode) }
        };

        return data;
    }

    public Dictionary<string, DataObject> SetMapName(string mapName)
    {
        MapName = mapName;

        return Get();
    }

    public Dictionary<string, DataObject> SetRelayCode(string relayCode)
    {
        RelayCode = relayCode;

        return Get();
    }

    public async Task UpdateLobbyData(string lobbyId)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions
        {
            Data = Get()
        };
        Debug.Log("UpdateLobbyData: " + options.Data["RelayCode"].Value + " - " + lobbyId);
        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
    }
}
