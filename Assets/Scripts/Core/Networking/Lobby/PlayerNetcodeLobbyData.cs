using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerNetcodeLobbyData : INetworkSerializable, IEquatable<PlayerNetcodeLobbyData>
{
    public FixedString32Bytes LobbyPlayerId;
    public ulong NetcodePlayerId;
    public FixedString32Bytes PlayerName;
    public bool IsReady;
    public TeamType Team;
    public Color playerColor;
    public int PlayerIndex;
    public int Progress;

    public PlayerNetcodeLobbyData(PlayerConnectionData playerConnectionData, ulong netcodePlayerId)
    {
        LobbyPlayerId = playerConnectionData.PlayerId;
        PlayerName = playerConnectionData.PlayerName ?? playerConnectionData.PlayerId;
        NetcodePlayerId = netcodePlayerId;
        IsReady = false;
        Team = TeamType.None;
        playerColor = Color.white;
        Progress = 0;
        PlayerIndex = 0;
    }

    public bool Equals(PlayerNetcodeLobbyData other)
    {
        return LobbyPlayerId == other.LobbyPlayerId &&
            NetcodePlayerId == other.NetcodePlayerId &&
            IsReady == other.IsReady &&
            Team == other.Team &&
            playerColor == other.playerColor &&
            PlayerName == other.PlayerName &&
            Progress == other.Progress &&
            PlayerIndex == other.PlayerIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref LobbyPlayerId);
        serializer.SerializeValue(ref NetcodePlayerId);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref Team);
        serializer.SerializeValue(ref playerColor);
        serializer.SerializeValue(ref Progress);
        serializer.SerializeValue(ref PlayerIndex);
    }
}