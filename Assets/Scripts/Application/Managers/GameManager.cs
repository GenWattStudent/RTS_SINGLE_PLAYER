using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>
{
    public bool IsDebug = true;

    private void Start()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject obj = new GameObject("EventSystem");
            obj.AddComponent<EventSystem>();
            obj.AddComponent<StandaloneInputModule>();
        }
    }
}