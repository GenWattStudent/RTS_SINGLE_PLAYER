using UnityEngine;
using UnityEngine.UIElements;

public class UIRTSActions : ToolkitHelper
{
    private Button targetButton;
    private Button cancelButton;
    public bool isSetTargetMode = false;
    public static UIRTSActions Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    void Start()
    {
        targetButton = GetButton("Target");
        cancelButton = GetButton("Cancel");
        targetButton.RegisterCallback<ClickEvent>(OnTargetButtonClick);
        cancelButton.RegisterCallback<ClickEvent>(ev => CancelTargetCommand());
    }

    private void OnTargetButtonClick(ClickEvent ev) {
        isSetTargetMode = !isSetTargetMode;
        Debug.Log("Target button click");
    }

    private void CancelTargeting() {
        isSetTargetMode = false;
    }

    private void CancelTargetCommand() {
        var selectedUnits = SelectionManager.selectedObjects;

        foreach (var unit in selectedUnits)
        {
            var attackScript = unit.GetComponent<Attack>();
            if (attackScript != null) attackScript.SetTargetPosition(Vector3.zero);
        }

        isSetTargetMode = false;
    }

    private void SelectTarget() {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            var selectedUnits = SelectionManager.selectedObjects;
            foreach (var unit in selectedUnits)
            {
                var attackScript = unit.GetComponent<Attack>();

                if (attackScript != null) attackScript.SetTargetPosition(hit.point);
            }

            isSetTargetMode = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isSetTargetMode) {
            if (Input.GetMouseButtonDown(0)) {
                SelectTarget();
            }

            if (Input.GetMouseButtonDown(1)) {
                CancelTargeting();
            }
        }
    }
}
