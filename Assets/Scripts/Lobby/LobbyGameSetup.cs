using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyGameSetup : ToolkitHelper
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

    public void Initialize()
    {
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
        if (LobbyManager.Instance.IsHost() || !LobbyManager.Instance.HasLobbyDataValue("MapName")) return;

        var mapName = LobbyManager.Instance.CurrentLobby.Data["MapName"].Value;
        var map = maps.Find(m => m.MapName == mapName);

        if (map != null && SelectedMap != map) SelectMap(map);
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

        currentMapName.text = map.MapName;
        currentMap.style.backgroundImage = new StyleBackground(map.MapImage);

        SelectedMap = map;
        OnMapSelected?.Invoke(SelectedMap);
    }

    public void Update()
    {
        UpdateLobbyMap();
        UpdateLobbyUI();
    }
}
