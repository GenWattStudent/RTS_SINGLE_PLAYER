using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyGameSetup : ToolkitHelper
{
    [SerializeField] private List<MapSo> maps = new();
    [SerializeField] private VisualTreeAsset mapItemTemplate;
    [SerializeField] private float updateInterval = 3.0f;
    [HideInInspector] public MapSo SelectedMap;

    private float updateTimer = 3.0f;
    private VisualElement currentMap;
    private Label currentMapName;
    private ScrollView mapList;
    private LobbyManager lobbyManager;
    private List<VisualElement> mapItems = new();

    public event Action<MapSo> OnMapSelected;

    public void Initialize()
    {
        CreateMapItems(maps);
        SelectMap(maps[0]);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        lobbyManager = FindAnyObjectByType<LobbyManager>();
        currentMap = GetVisualElement("CurrentMap");
        mapList = root.Q<ScrollView>("Maps");
        currentMapName = GetLabel("CurrentMapName");
    }

    public void SetHost(bool isHost)
    {
        if (isHost)
        {
            mapList.style.display = DisplayStyle.Flex;
        }
        else
        {
            mapList.style.display = DisplayStyle.None;
        }
    }

    private void OnDisable()
    {
        mapList.Clear();
    }

    private void CreateMapItems(List<MapSo> maps)
    {
        mapList.Clear();
        foreach (var map in maps)
        {
            CreateMapItem(map);
        }
    }

    private void UpdateLobbyMap()
    {
        if (
            !lobbyManager.CurrentLobby.Data.ContainsKey("MapName") &&
            lobbyManager.CurrentLobby.Data["MapName"].Value != default
         ) return;
        var mapName = lobbyManager.CurrentLobby.Data["MapName"].Value;
        var map = maps.Find(m => m.MapName == mapName);
        Debug.Log($"UpdateLobbyMap {mapName}");
        if (map != null) SelectMap(map);
    }

    private void UpdateLobbyUI()
    {
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

        currentMapName.text = map.MapName;
        currentMap.style.backgroundImage = new StyleBackground(map.MapImage);

        SelectedMap = map;
        OnMapSelected?.Invoke(SelectedMap);
    }

    private void Update()
    {
        if (lobbyManager.CurrentLobby == null) return;

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            UpdateLobbyMap();
            UpdateLobbyUI();
            updateTimer = updateInterval;
        }
    }
}
