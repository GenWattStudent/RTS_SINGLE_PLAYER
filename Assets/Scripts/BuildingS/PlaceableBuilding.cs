using System.Collections.Generic;
using UnityEngine;

public class PlaceableBuilding : MonoBehaviour
{
  [HideInInspector] public List<Collider> colliders;

  private void OnTriggerEnter(Collider other)
  {
    Debug.Log("OnTriggerEnter " + colliders.Count);
    if (other.tag == "Building")
    {
        colliders.Add(other);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    Debug.Log("OnTriggerExit " + colliders.Count);
    if (other.tag == "Building")
    {
        colliders.Remove(other);
    }
  }
}
