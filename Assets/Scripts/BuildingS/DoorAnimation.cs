using System.Collections;
using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    private TankBuilding spawnerBuilding;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        spawnerBuilding = GetComponentInParent<TankBuilding>();
        spawnerBuilding.OnSpawnUnit += OpenDoor;
    }

    private void OpenDoor(UnitSo unitSo, Unit unit)
    {
        animator.SetBool("isOpen", true);

        // calculate time when unit will be in unit move point
        float timeToMove = Vector3.Distance(unit.transform.position, spawnerBuilding.unitMovePoint.position) / unitSo.speed;
        StartCoroutine(CloseDoorAfterDelay(timeToMove));
    }

    IEnumerator CloseDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool("isOpen", false);
    }
}
