using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingSo buildingSo;
    public AttackableSo attackableSo;
    public BuildingLevelable buildingLevelable;

    private void Start() {
        buildingLevelable = GetComponent<BuildingLevelable>();
    }
}
