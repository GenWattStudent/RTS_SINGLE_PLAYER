using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance {
        get {

            lock (typeof(T)) {
                if (instance == null) {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null) {
                        var singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    }
                }

                return instance;
            }
        }
    }

    private static bool applicationIsQuitting = false;

    public virtual void Awake() {
        if (instance == null) {
            instance = this as T;
        } else {
            Destroy(gameObject);
        }
    }

    public void OnDestroy() {
        applicationIsQuitting = true;
    }
}
