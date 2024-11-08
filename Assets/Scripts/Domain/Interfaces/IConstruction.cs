using UnityEngine;

public interface IWorkerConstruction
{
    void AddWorker(Worker worker);
    void RemoveWorker(Worker worker);

    Transform transform { get; }
    T GetComponent<T>();
}
