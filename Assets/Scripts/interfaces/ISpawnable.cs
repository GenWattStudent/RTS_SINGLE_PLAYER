
public interface ISpawnerBuilding
{
    void AddUnitToQueue(UnitSo unit);
    float GetSpawnTimer();
    UnitSo GetCurrentSpawningUnit();
    int GetUnitQueueCountByName(string unitName);
}
