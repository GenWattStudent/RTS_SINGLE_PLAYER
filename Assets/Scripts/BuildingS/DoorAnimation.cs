using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DoorAnimation : NetworkBehaviour
{
    private Spawner spawnerBuilding;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spawnerBuilding = GetComponentInParent<Spawner>();
        spawnerBuilding.OnSpawnUnit += OpenDoor;
    }

    private void OpenDoor(UnitSo unitSo, Unit unit)
    {
        if (!IsServer) return;
        animator.SetBool("isOpen", true);
        // calculate time when unit will be in unit move point
        float timeToMove = Vector3.Distance(unit.transform.position, spawnerBuilding.unitMovePoint.position) / unitSo.speed;
        StartCoroutine(CloseDoorAfterDelay(unit, timeToMove));
    }

    IEnumerator CloseDoorAfterDelay(Unit unit, float delay)
    {
        if (!IsServer) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < delay)
        {
            if (!spawnerBuilding.IsInsideSpawner(unit.transform.position))
            {
                break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Wait until the unit leaves the spawner
        while (spawnerBuilding.IsInsideSpawner(unit.transform.position))
        {
            yield return null;
        }

        animator.SetBool("isOpen", false);
    }
}
