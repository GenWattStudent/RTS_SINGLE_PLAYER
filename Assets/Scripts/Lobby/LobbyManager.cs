using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbyManager : MonoBehaviour
{
    [SerializeField] private float hearbeatInterval = 25.0f;
    public int maxPlayers = 2;
    private static LobbyManager instance;
    public Lobby CurrentLobby;
    private float heartbeatTimer = 0.0f;
    public bool isSingInCompleted = false;

    public static LobbyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LobbyManager();
            }
            return instance;
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignInCompleted;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        if (!isSingInCompleted) return;
        Heartbeat();
    }

    private void OnSignInCompleted()
    {
        isSingInCompleted = true;
        Debug.Log("Sign in completed");
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers)
    {
        CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
        Debug.Log("Lobby created: " + CurrentLobby.Id);
    }

    public async Task<List<Lobby>> GetAll()
    {
        var lobbies = await Lobbies.Instance.QueryLobbiesAsync();
        return lobbies.Results;
    }

    private async void Heartbeat()
    {
        if (CurrentLobby == null) return;

        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer < hearbeatInterval) return;

        heartbeatTimer = 0.0f;
        await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
    }
}

