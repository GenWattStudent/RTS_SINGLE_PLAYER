using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create PowerUp", menuName = "RTS/PowerUp")]
public class PowerUpSo : ScriptableObject
{
    public string Name;
    public float Duration;
    public float Cooldown;
    public List<Stat> Stats;
    public Sprite Icon;
    public GameObject Prefab;

    public bool IsPermanent = false;
    public bool IsStackable = true;
    public bool IsPercentage = true;
    public bool IsAbility = false;

    public bool HasCooldown => Cooldown > 0;
}
