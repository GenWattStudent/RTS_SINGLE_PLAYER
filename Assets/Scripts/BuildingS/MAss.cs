using UnityEngine;

public class MAss : MonoBehaviour
{
    public BuildingSo buildingSo;
    public int income;
    public float incomeInterval;
    public float incomeTimer;
    public int currentIncome = 0;
    public int totalIncome = 0;

    private void Income() {
        currentIncome += income;
        incomeTimer = incomeInterval;
        totalIncome += income;
        UIStorage.Instance.IncreaseMass(income);
    }

    // Start is called before the first frame update
    void Start()
    {
        income = buildingSo.income;
        incomeInterval = buildingSo.incomeInterval;
        incomeTimer = incomeInterval;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        incomeTimer -= Time.fixedDeltaTime;

        if (incomeTimer <= 0) {
            Income();
        }
    }
}
