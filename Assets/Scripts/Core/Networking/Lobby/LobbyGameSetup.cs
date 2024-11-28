using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class LobbyGameSetup : NetworkToolkitHelper
{
    [SerializeField] private List<MapSo> maps = new();
    [SerializeField] private VisualTreeAsset mapItemTemplate;
    [HideInInspector] public MapSo SelectedMap;

    private VisualElement currentMap;
    private Label currentMapName;
    private ScrollView mapList;
    private VisualElement mapBox;
    private List<VisualElement> mapItems = new();

    public event Action<MapSo> OnMapSelected;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId && !NetworkManager.Singleton.IsServer)
        {
            SelectedMap = maps.Find(m => m.MapName == LobbyRoomService.Instance.lobbyNetcodeDataHandler.GetMapName());
            UpdateMapUI(SelectedMap);
            UpdateLobbyUI();
        }
    }

    public void Initialize()
    {
        LobbyRoomService.Instance.lobbyNetcodeDataHandler.lobbyNetcodeData.OnValueChanged += HandleLobbyDataChange;

        if (!LobbyManager.Instance.IsHost())
        {
            mapBox.style.display = DisplayStyle.None;
        }
        else
        {
            CreateMapItems(maps);
            SelectMap(maps[0]);
            mapBox.style.display = DisplayStyle.Flex;
        }
    }

    private void HandleLobbyDataChange(LobbyNetcodeData previousValue, LobbyNetcodeData newValue)
    {
        SelectedMap = maps.Find(m => m.MapName == newValue.MapName.ToString());
        Debug.Log($"Map changed to {SelectedMap.MapName}");
        UpdateMapUI(SelectedMap);
        UpdateLobbyUI();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        currentMap = GetVisualElement("CurrentMap");
        mapList = root.Q<ScrollView>("Maps");
        currentMapName = GetLabel("CurrentMapName");
        mapBox = GetVisualElement("MapBox");
    }

    private void OnDisable()
    {
        mapList.Clear();
        LobbyRoomService.Instance.lobbyNetcodeDataHandler.lobbyNetcodeData.OnValueChanged -= HandleLobbyDataChange;
    }

    private void CreateMapItems(List<MapSo> maps)
    {
        mapList.Clear();
        foreach (var map in maps)
        {
            CreateMapItem(map);
        }
    }

    private void UpdateLobbyUI()
    {
        if (SelectedMap == null) return;

        var selectedMapIndex = maps.FindIndex(m => m.MapName == SelectedMap.MapName);

        for (int i = 0; i < mapItems.Count; i++)
        {
            if (i == selectedMapIndex) mapItems[i].AddToClassList("active");
            else mapItems[i].RemoveFromClassList("active");
        }
    }

    private void CreateMapItem(MapSo map)
    {
        var mapItem = mapItemTemplate.CloneTree();
        mapItem.Q<Label>("MapName").text = map.MapName;
        mapItem.Q<VisualElement>("MapImage").style.backgroundImage = new StyleBackground(map.MapImage);

        mapItem.RegisterCallback<ClickEvent>(e => SelectMap(map));
        mapList.Add(mapItem);
        mapItems.Add(mapItem.Q<Button>("MapItem"));
    }

    private void SelectMap(MapSo map)
    {
        if (SelectedMap == map) return;
        SelectedMap = map;
        OnMapSelected?.Invoke(SelectedMap);
    }

    private void UpdateMapUI(MapSo map)
    {
        currentMapName.text = map.MapName;
        currentMap.style.backgroundImage = new StyleBackground(map.MapImage);
    }
}
