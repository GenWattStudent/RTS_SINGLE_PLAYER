using System;
using Unity.Collections;
using Unity.Netcode;

public struct LobbyNetcodeData : INetworkSerializable, IEquatable<LobbyNetcodeData>
{
    public FixedString32Bytes MapName;

    public bool Equals(LobbyNetcodeData other)
    {
        return MapName == other.MapName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MapName);
    }
}
