using UnityEngine;

[CreateAssetMenu(fileName = "Laser", menuName = "ScriptableObjects/Laser")]
public class LaserSo : ScriptableObject
{
    public string laserName;
    public float damage;
    public float damgeInterval;
    public Material laserMaterial;
}
