using UnityEngine;

[CreateAssetMenu(fileName = "Laser", menuName = "ScriptableObjects/Laser")]
public class LaserSo : ScriptableObject
{
    public string LaserName;
    public float Damage;
    public float AttackSpeed;
    public Material LaserMaterial;
}
