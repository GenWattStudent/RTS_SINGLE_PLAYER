using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation.Formations
{

    /// <summary>
    /// Formation that positions units in a rectangle with specified spacing
    /// and maximal column count.
    /// </summary>
    public struct RectangleFormation : IFormation
    {

        /// <summary>
        /// Returns the column count which represents the max
        /// unit number in a single row.
        /// </summary>
        public int ColumnCount { get; private set; }

        private float spacing;
        private bool centerUnits;
        private bool pivotInMiddle;

        /// <summary>
        /// Instantiates rectangle formation.
        /// </summary>
        /// <param name="columnCount">Maximal number of columns per row (there
        /// are less rows if number of units is smaller than this number).</param>
        /// <param name="spacing">Specifies spacing between units.</param>
        /// <param name="centerUnits">Specifies if units should be centered if
        /// they do not fill the full space of the row.</param>
        /// <param name="pivotInMiddle">Specifies if the pivot of the formation is
        /// in the middle of units. By default it is in first row of the formation.
        /// If this is set to true, rotation of formation will be in the center.</param>
        public RectangleFormation(
            int columnCount,
            float spacing,
            bool centerUnits = true,
            bool pivotInMiddle = false)
        {
            ColumnCount = columnCount;
            this.spacing = spacing;
            this.centerUnits = centerUnits;
            this.pivotInMiddle = pivotInMiddle;
        }

        public List<Vector3> GetPositions(int unitCount)
        {
            List<Vector3> unitPositions = new List<Vector3>();
            var unitsPerRow = Mathf.Min(ColumnCount, unitCount);
            float offsetX = (unitsPerRow - 1) * spacing / 2f;

            if (unitsPerRow == 0)
            {
                return new List<Vector3>();
            }

            float rowCount = unitCount / ColumnCount + (unitCount % ColumnCount > 0 ? 1 : 0);
            float x, y, column;
            int firstIndexInRow;

            for (int row = 0; unitPositions.Count < unitCount; row++)
            {
                // Check if centering is enabled and if row has less than maximum
                // allowed units within the row.
                firstIndexInRow = row * ColumnCount;
                if (centerUnits &&
                    row != 0 &&
                    firstIndexInRow + ColumnCount > unitCount)
                {
                    // Alter the offset to center the units that do not fill the row
                    var emptySlots = firstIndexInRow + ColumnCount - unitCount;
                    offsetX -= emptySlots / 2f * spacing;
                }

                for (column = 0; column < ColumnCount; column++)
                {
                    if (firstIndexInRow + column < unitCount)
                    {
                        x = column * spacing - offsetX;
                        y = row * spacing;

                        Vector3 newPosition = new Vector3(x, 0, -y);
                        unitPositions.Add(newPosition);
                    }
                    else
                    {
                        if (pivotInMiddle)
                            UnitFormationHelper.ApplyFormationCentering(ref unitPositions, rowCount, spacing);
                        return unitPositions;
                    }
                }
            }

            if (pivotInMiddle)
                UnitFormationHelper.ApplyFormationCentering(ref unitPositions, rowCount, spacing);
            return unitPositions;
        }

    }

}