using UnityEngine;

[CreateAssetMenu(fileName = "Gather Item", menuName = "RTS/GatherItem")]
public class GatherItemSo : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;

    public int maxValue;
    public int minValue;

    public ResourceSO resourceSO;
}
