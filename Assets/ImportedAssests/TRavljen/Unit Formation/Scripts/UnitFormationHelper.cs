using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRavljen.UnitFormation
{
    public static class UnitFormationHelper
    {
        /// <summary>
        /// Applies offset to the Z axes on positions in order to move positions
        /// from pivot in front of formation, to pivot in center of the formation.
        /// </summary>
        /// <param name="positions">Current positions, method will update the reference values.</param>
        /// <param name="rowCount">Row count produced with formation.</param>
        /// <param name="rowSpacing">Spacing between each row.</param>
        public static void ApplyFormationCentering(ref List<Vector3> positions, float rowCount, float rowSpacing)
        {
            float offsetZ = Mathf.Max(0, (rowCount - 1) * rowSpacing / 2);

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                pos.z += offsetZ;
                positions[i] = pos;
            }
        }

        /// <summary>
        /// Generates random "noise" for the position. In reality takes random
        /// range in the offset, does not use actual Math noise methods.
        /// </summary>
        /// <param name="factor">Factor for which the position can be offset.</param>
        /// <returns>Returns local offset for axes X and Z.</returns>
        public static Vector3 GetNoise(float factor)
        {
            return new Vector3(Random.Range(-factor, factor), 0, Random.Range(-factor, factor));
        }
    }
}