using System;
using UnityEngine;

public interface IWorkerConstruction
{
    void AddWorker(Worker worker);
    void RemoveWorker(Worker worker);
    void RemoveWorkers();

    Transform transform { get; }

    T GetComponent<T>();

    public event Action OnFinshed;
}
