using System.Collections.Generic;
using UnityEngine;
using TRavljen.UnitFormation.Formations;
using TRavljen.UnitFormation;
using UnityEngine.UI;

namespace TRavljen.UnitFormation.Demo
{

    public class UnitFormationControls : MonoBehaviour
    {

        #region Public Properties

        /// <summary>
        /// List of units in the scene
        /// </summary>
        public List<GameObject> units = new List<GameObject>();

        #endregion

        #region Inspector Properties

        /// <summary>
        /// Specifies the layer mask used for mouse point raycasts in order to
        /// find the drag positions in world/scene.
        /// </summary>
        [SerializeField] private LayerMask groundLayerMask;

        /// <summary>
        /// Specifies the line renderer used for rendering the mouse drag line
        /// that indicates the unit facing direction.
        /// </summary>
        [SerializeField] private LineRenderer LineRenderer;

        /// <summary>
        /// Specifies the unit count that will be generated for the scene.
        /// May be adjusted in realtime.
        /// </summary>
        [SerializeField] private Slider UnitCountSlider;

        /// <summary>
        /// Specifies the unit spacing that will be used to generate formation
        /// positions.
        /// </summary>
        [SerializeField] private Slider UnitSpacingSlider;

        [SerializeField] private Slider RectangleColumnCountSlider;

        /// <summary>
        /// Specifies the <see cref="Text"/> used to represent the unit count
        /// selected by <see cref="UnitCountSlider"/>.
        /// </summary>
        [SerializeField] private Text UnitCountText;

        /// <summary>
        /// Specifies the <see cref="Text"/> used to represent the unit spacing
        /// selected by <see cref="UnitSpacingSlider"/>.
        /// </summary>
        [SerializeField] private Text UnitSpacingText;

        [SerializeField] private Text RectangleColumnCountText;

        [SerializeField] private GameObject UnitPrefab = null;

        #endregion

        #region Private Properties

        private IFormation currentFormation;

        private bool isDragging = false;

        private bool pivotInMiddle = false;
        private bool noiseEnabled = false;

        private int unitCount => (int)UnitCountSlider.value;
        private int rectangleColumnCount => (int)RectangleColumnCountSlider.value;
        private float unitSpacing => UnitSpacingSlider.value;

        #endregion

        private void Start()
        {
            LineRenderer.enabled = false;
            SetUnitFormation(new LineFormation(unitSpacing));

            // Initial UI update
            UpdateUnitCountText();
            UpdateUnitSpacing();
            UpdateRectangleColumnCountText();
        }

        private void Update()
        {
            if (units.Count < unitCount)
            {
                for (int index = units.Count; index < unitCount; index++)
                {
                    var gameObject = Instantiate(
                        UnitPrefab, transform.position, Quaternion.identity);
                    units.Insert(index, gameObject);
                }

                ApplyCurrentUnitFormation();
            }
            else if (units.Count > unitCount)
            {
                for (int index = units.Count - 1; index >= unitCount; index--)
                {
                    var gameObject = units[index];
                    units.RemoveAt(index);
                    Destroy(gameObject);
                }

                ApplyCurrentUnitFormation();
            }

            if (units.Count > 0)
            {
                HandleMouseDrag();
            }
        }

        private void HandleMouseDrag()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100, groundLayerMask))
                {
                    LineRenderer.enabled = true;
                    isDragging = true;

                    LineRenderer.SetPosition(0, hit.point);
                    LineRenderer.SetPosition(1, hit.point);
                }
            }
            else if (Input.GetKey(KeyCode.Mouse1) & isDragging)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100, groundLayerMask))
                {
                    LineRenderer.SetPosition(1, hit.point);

                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse1) && isDragging)
            {
                isDragging = false;
                LineRenderer.enabled = false;
                ApplyCurrentUnitFormation();
            }
        }

        private void ApplyCurrentUnitFormation()
        {
            var direction = LineRenderer.GetPosition(1) - LineRenderer.GetPosition(0);

            UnitsFormationPositions formationPos;

            // Check if mouse drag was NOT minor, then we can calculate angle
            // (direction) for the mouse drag.
            if (direction.magnitude > 0.8f)
            {
                var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                var newPositions = FormationPositioner.GetAlignedPositions(
                    units.Count, currentFormation, LineRenderer.GetPosition(0), angle);

                formationPos = new UnitsFormationPositions(newPositions, angle);
            }
            else
            {
                var currentPositions = units.ConvertAll(obj => obj.transform.position);
                formationPos = FormationPositioner.GetPositions(
                    currentPositions, currentFormation, LineRenderer.GetPosition(0));
            }

            for (int index = 0; index < units.Count; index++)
            {
                Vector3 pos = formationPos.UnitPositions[index];
                if (noiseEnabled)
                {
                    pos += UnitFormationHelper.GetNoise(0.2f);
                }

                if (units[index].TryGetComponent(out FormationUnit unit))
                {
                    unit.SetTargetDestination(pos, formationPos.FacingAngle);
                }
            }
        }

        private void SetUnitFormation(IFormation formation)
        {
            currentFormation = formation;
            ApplyCurrentUnitFormation();
        }

        #region User Interactions

        public void OnNoiseToggleChanged(bool newState)
        {
            noiseEnabled = newState;

            ReinstantiateFormation();
            ApplyCurrentUnitFormation();
        }

        public void OnPivotToggleChanged(bool newState)
        {
            pivotInMiddle = newState;

            ReinstantiateFormation();
            ApplyCurrentUnitFormation();
        }

        public void LineFormationSelected() =>
            SetUnitFormation(new LineFormation(unitSpacing));

        public void CircleFormationSelected() =>
            SetUnitFormation(new CircleFormation(unitSpacing));

        public void TriangleFormationSelected() =>
            SetUnitFormation(new TriangleFormation(unitSpacing));

        public void ConeFormationSelected() =>
            SetUnitFormation(new ConeFormation(unitSpacing, pivotInMiddle));

        public void RectangleFormationSelected() =>
            SetUnitFormation(new RectangleFormation(rectangleColumnCount, unitSpacing, true, pivotInMiddle));

        public void UpdateRectangleColumnCountText()
        {
            RectangleColumnCountText.text = "Units per ROW: " + rectangleColumnCount;

            if (currentFormation is RectangleFormation)
            {
                ReinstantiateFormation();
                ApplyCurrentUnitFormation();
            }
        }

        public void UpdateUnitCountText()
        {
            UnitCountText.text = "Unit Count: " + unitCount;
        }

        public void UpdateUnitSpacing()
        {
            UnitSpacingText.text = $"Unit Spacing: {unitSpacing.ToString(("0.00"))}";

            ReinstantiateFormation();
            ApplyCurrentUnitFormation();
        }

        /// <summary>
        /// Instantiates a new formation based on the current type with the new
        /// configurations applied from UI.
        /// </summary>
        private void ReinstantiateFormation()
        {
            if (currentFormation is LineFormation)
            {
                currentFormation = new LineFormation(unitSpacing);
            }
            else if (currentFormation is RectangleFormation rectangleFormation)
            {
                currentFormation = new RectangleFormation(
                    rectangleColumnCount, unitSpacing, true, pivotInMiddle);
            }
            else if (currentFormation is CircleFormation)
            {
                currentFormation = new CircleFormation(unitSpacing);
            }
            else if (currentFormation is TriangleFormation)
            {
                currentFormation = new TriangleFormation(unitSpacing, pivotInMiddle: pivotInMiddle);
            }
            else if (currentFormation is ConeFormation)
            {
                currentFormation = new ConeFormation(unitSpacing, pivotInMiddle);
            }
        }

        #endregion

    }

}
