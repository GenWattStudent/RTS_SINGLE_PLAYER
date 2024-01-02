using System.Collections.Generic;
using UnityEngine;

public class PlaceableBuilding : MonoBehaviour
{
  [HideInInspector] public List<Collider> colliders;

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Terrain") || other.CompareTag("ForceField")) return;
    Debug.Log("OnTriggerEnter");
    colliders.Add(other);
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.CompareTag("Terrain") || other.CompareTag("ForceField")) return;
    Debug.Log("OnTriggerExit");
    colliders.Remove(other);
  }

  private void Update()
  {
    Debug.Log(colliders.Count);
  }
}
