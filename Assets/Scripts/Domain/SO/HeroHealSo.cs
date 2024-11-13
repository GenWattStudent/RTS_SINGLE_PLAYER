using System.Collections;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Hero heal", menuName = "RTS/HeroHeal")]
public class HeroHealSo : SkillTreeSo
{
    public int healAmount;
    public float healInterval;
    public float healDuration;
    public bool isHealOverTime => healInterval > 0;
    public float healRange;

    public override void Activate(Unit unit)
    {
        if (unit.Damagable != null)
        {
            if (isHealOverTime)
            {
                unit.StartCoroutine(HealOverTime(unit));
            }
            else
            {
                Heal(unit);
            }

            var skillInstance = Instantiate(skillPrefab, unit.transform);
            skillInstance.GetComponent<NetworkObject>().SpawnWithOwnership(unit.OwnerClientId);
            skillInstance.transform.SetParent(unit.transform);
            unit.StartCoroutine(DestroyAfterDuration(skillInstance, healDuration));
        }
    }

    private IEnumerator DestroyAfterDuration(GameObject skillInstance, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (skillInstance != null && skillInstance.GetComponent<NetworkObject>().IsSpawned)
        {
            skillInstance.GetComponent<NetworkObject>().Despawn();
            Destroy(skillInstance);
        }
    }

    private void Heal(Unit unit)
    {
        var hits = Physics.OverlapSphere(unit.transform.position, healRange);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Damagable damagable) && damagable.IsTeamMate(unit.Damagable))
            {
                damagable.TakeDamage(-healAmount);
            }
        }
    }

    private IEnumerator HealOverTime(Unit unit)
    {
        float elapsedTime = 0f;
        while (elapsedTime < healDuration)
        {
            Heal(unit);
            elapsedTime += healInterval;
            yield return new WaitForSeconds(healInterval);
        }
    }
}
