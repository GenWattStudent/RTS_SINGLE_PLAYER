using System;
using Unity.Netcode;

public interface ISpawnerBuilding
{
    void AddUnitToQueueServerRpc(int index, ServerRpcParams rpcParams = default);
    float GetSpawnTimer();
    UnitSo GetCurrentSpawningUnit();
    int GetUnitQueueCountByName(string unitName);
}
