using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DoorAnimation : NetworkBehaviour
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
        if (!IsServer) return;
        animator.SetBool("isOpen", true);

        // calculate time when unit will be in unit move point
        float timeToMove = Vector3.Distance(unit.transform.position, spawnerBuilding.unitMovePoint.position) / unitSo.speed;
        StartCoroutine(CloseDoorAfterDelay(timeToMove));
    }

    IEnumerator CloseDoorAfterDelay(float delay)
    {
        if (!IsServer) yield break;
        yield return new WaitForSeconds(delay);
        animator.SetBool("isOpen", false);
    }
}
