using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units in a cone shape with specified spacing.
    /// </summary>
    public struct ConeFormation : IFormation
    {
        private float spacing;
        private bool pivotInMiddle;

        /// <summary>
        /// Instantiates cone formation.
        /// </summary>
        /// <param name="spacing">Specifies spacing between units.</param>
        /// <param name="pivotInMiddle">Specifies if the pivot of the formation is
        /// in the middle of units. By default it is in first row of the formation.
        /// If this is set to true, rotation of formation will be in the center.</param>
        public ConeFormation(float spacing, bool pivotInMiddle = true)
        {
            this.spacing = spacing;
            this.pivotInMiddle = pivotInMiddle;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();

            // Offset starts at 0, then each row is applied change for half of spacing
            float currentRowOffset = 0f;
            float x, z;
            int columnsInRow;
            int row;

            for (row = 0; unitPositions.Count < unitCount; row++)
            {
                columnsInRow = row + 1;

                x = 0 * spacing + currentRowOffset;
                z = row * spacing;

                unitPositions.Add(new Vector3(x, 0, -z));

                if (unitPositions.Count < unitCount && columnsInRow > 1)
                {
                    x = (columnsInRow - 1) * spacing + currentRowOffset;
                    z = row * spacing;

                    unitPositions.Add(new Vector3(x, 0, -z));
                }

                currentRowOffset -= spacing / 2;
            }

            if (pivotInMiddle)
                UnitFormationHelper.ApplyFormationCentering(ref unitPositions, row, spacing);

            return unitPositions;
        }

    }
}