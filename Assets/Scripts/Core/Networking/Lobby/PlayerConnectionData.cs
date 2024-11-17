
using System;
using System.Text;
using UnityEngine;

[Serializable]
public class PlayerConnectionData
{
    public string PlayerId;
    public string PlayerName;

    public PlayerConnectionData(string playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public byte[] ToByteArray()
    {
        string json = JsonUtility.ToJson(this);
        return Encoding.UTF8.GetBytes(json);
    }

    public static PlayerConnectionData FromByteArray(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        return JsonUtility.FromJson<PlayerConnectionData>(json);
    }
}
