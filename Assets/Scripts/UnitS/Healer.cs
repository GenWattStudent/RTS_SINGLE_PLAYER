using UnityEngine;

public class Healer : MonoBehaviour
{
    public Damagable target;
    public DamagableSo damagableSo;
    private float healRate = 1f;
    private UnitMovement unitMovement;
    private Laser laser;
    private float healPoints = 0f;

    public void Heal(Damagable target)
    {
        if (target.health >= target.maxHealth) {
            SetTarget(null);
            return;
        }

        target.Heal(healPoints * Time.deltaTime);
    }

    public void SetTarget(Damagable target)
    {
        this.target = target;
        laser.SetTarget(target);

        if (target == null) return;
        target.OnDead += () => SetTarget(null);
    }

    private bool IsInRange()
    {
        if (target == null) return false;
        var colliders = Physics.OverlapSphere(transform.position, damagableSo.attackRange);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == target.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    private void MoveToTarget()
    {
        if (target == null) return;
        // move to target to be in distance to heal take offset in thye account
        var offset = 1f;
        var distance = Vector3.Distance(transform.position, target.transform.position);
        var distanceToMove = distance - damagableSo.attackRange + offset;
        var direction = (target.transform.position - transform.position).normalized;
        var destination = transform.position + direction * distanceToMove;

        unitMovement.MoveTo(destination);
    }

    // Start is called before the first frame update
    void Start()
    {
        unitMovement = GetComponent<UnitMovement>();
        laser = GetComponent<Laser>();
        healRate = damagableSo.attackSpeed;
        healPoints = damagableSo.attackDamage / healRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;;

        if (IsInRange() == false) {
            MoveToTarget();
        }

        if (unitMovement != null)  unitMovement.RotateToTarget(target.transform.position);
        Heal(target);
    }
}
