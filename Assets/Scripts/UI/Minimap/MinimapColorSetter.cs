using Unity.Netcode;
using UnityEngine.UI;

public class MinimapColorSetter : NetworkBehaviour
{
    private PlayerController playerController;
    private Image image;

    void Start()
    {
        playerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
        image = GetComponent<Image>();
        image.color = playerController.playerData.playerColor;
    }
}
