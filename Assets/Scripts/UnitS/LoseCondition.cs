using UnityEngine;

public class LoseCondition : MonoBehaviour
{
    private Damagable damagable;

    private void Start() {
        damagable = GetComponent<Damagable>();
        damagable.OnDead += OnDead;
    }

    private void OnDead() {
        GameResult.Instance.Defeat();
    }
}
