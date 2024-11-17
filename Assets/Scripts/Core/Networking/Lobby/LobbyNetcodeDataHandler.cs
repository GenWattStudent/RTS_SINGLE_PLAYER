using Unity.Netcode;
using UnityEngine;

public class LobbyNetcodeDataHandler : NetworkBehaviour
{
    public NetworkVariable<LobbyNetcodeData> lobbyNetcodeData;

    private void Awake()
    {
        lobbyNetcodeData = new NetworkVariable<LobbyNetcodeData>(new LobbyNetcodeData());
    }

    public void SetMapName(string mapName)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var newData = lobbyNetcodeData.Value;
            newData.MapName = mapName;
            lobbyNetcodeData.Value = newData;
        }
    }

    public string GetMapName()
    {
        return lobbyNetcodeData.Value.MapName.ToString();
    }
}
