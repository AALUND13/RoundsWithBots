using UnityEngine;

namespace RoundsWithBots.Utils {
    public static class MathUtils {
        public static float CalculatePointsStraightness(Vector2 pointOne, Vector2 pointTwo, Vector2 pointThree) {
            Vector2 line = pointThree - pointOne;

            if (line.sqrMagnitude < Mathf.Epsilon) {
                return Vector2.Distance(pointTwo, pointOne);
            }

            float cross = Mathf.Abs(
                (pointTwo.x - pointOne.x) * line.y -
                (pointTwo.y - pointOne.y) * line.x
            );

            return cross / line.magnitude;
        }
    }
}
