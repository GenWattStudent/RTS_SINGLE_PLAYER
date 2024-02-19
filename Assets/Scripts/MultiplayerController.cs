using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData
{
    public int teamId;
    public Color playerColor;
    public Material playerMaterial;
    public List<Unit> units = new();
    public List<Building> buildings = new();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);
}

[Serializable]
public class PlayerVisualData
{
    public Color playerColor;
    public Material playerMaterial;
    public Transform spawnPosition;
}

public class MultiplayerController : NetworkBehaviour
{
    public List<PlayerVisualData> playerMaterials = new();
    public static MultiplayerController Instance;

    private void Awake()
    {
        Instance = this;
    }
}
