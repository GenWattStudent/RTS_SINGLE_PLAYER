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

    private event Action<MapSo> OnMapSelected;

    protected override void OnEnable()
    {
        base.OnEnable();
        currentMap = GetVisualElement("CurrentMap");
        mapList = root.Q<ScrollView>("Maps");
        currentMapName = GetLabel("CurrentMapName");
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

    private void CreateMapItem(MapSo map)
    {
        var mapItem = mapItemTemplate.CloneTree();
        mapItem.Q<Label>("MapName").text = map.MapName;
        mapItem.Q<VisualElement>("MapImage").style.backgroundImage = new StyleBackground(map.MapImage);

        mapItem.RegisterCallback<ClickEvent>(e => SelectMap(map));
        mapList.Add(mapItem);
    }

    private void Start()
    {
        CreateMapItems(maps);
        SelectMap(maps[0]);
    }

    private void SelectMap(MapSo map)
    {
        if (SelectedMap == map) return;

        currentMapName.text = map.MapName;
        currentMap.style.backgroundImage = new StyleBackground(map.MapImage);

        SelectedMap = map;
        OnMapSelected?.Invoke(SelectedMap);
    }
}
