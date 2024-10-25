using System;
using Unity.Netcode;

public interface ISpawnerBuilding
{
    void AddUnitToQueueServerRpc(int index, ServerRpcParams rpcParams = default);
    UnitSo GetCurrentSpawningUnit();
    int GetUnitQueueCountByName(string unitName);
}
