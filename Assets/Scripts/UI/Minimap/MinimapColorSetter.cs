using Unity.Netcode;
using UnityEngine.UI;

public class MinimapColorSetter : NetworkBehaviour
{
    private void Start()
    {
        var color = UnityEngine.Color.white;
        if (!GameManager.Instance.IsDebug)
        {
            var lobbyPlayerData = LobbyPlayersHandler.Instance.GetPlayerData(OwnerClientId);
            color = lobbyPlayerData.Value.playerColor;
        }
        else
        {
            color = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerColor;
        }

        var image = GetComponent<Image>();

        image.color = color;
    }
}
