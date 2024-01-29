using System;

public interface ISpawnerBuilding
{
    float totalSpawnTime { get; set; }
    void AddUnitToQueue(UnitSo unit);
    float GetSpawnTimer();
    UnitSo GetCurrentSpawningUnit();
    int GetUnitQueueCountByName(string unitName);
}
