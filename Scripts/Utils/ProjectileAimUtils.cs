using UnityEngine;

namespace RoundsWithBots.Utils {
    public class MoveTransformSimulator {
        public Vector2 Start;
        public Vector2 Velocity;
        public Vector2 Position;

        public float Gravity;
        public float Drag;
        public float DragMinSpeed;
        public float Multiplier;

        public MoveTransformSimulator(
            Vector2 start,
            Vector2 velocity,
            float gravity,
            float drag,
            float dragMinSpeed,
            float multiplier
        ) {
            Start = start;
            Position = start;
            Velocity = velocity;

            Gravity = gravity;
            Drag = drag;
            DragMinSpeed = dragMinSpeed;
            Multiplier = multiplier;
        }

        public void Step(float simulationStep) {
            if (Gravity != 0f) {
                Velocity += Gravity * Vector2.down * simulationStep * Multiplier;
            }

            if (Velocity.magnitude > 2f && Velocity.magnitude > DragMinSpeed) {
                Velocity -= Velocity * Mathf.Clamp(Drag * simulationStep * Mathf.Clamp(Multiplier, 0f, 1f), 0f, 1f);
            }

            Position += Velocity * simulationStep * Multiplier;
        }
    }

    public static class ProjectileAimUtils {
        public static bool TryGetAimDirection(
            Vector2 start,
            Vector2 target,
            float speed,
            float gravity,
            float drag,
            float dragMinSpeed,
            float multiplier,
            float maxTime,
            float simulationStep,
            out Vector2 direction
        ) {
            direction = Vector2.zero;

            Vector2 toTarget = target - start;

            if (toTarget.sqrMagnitude < 0.0001f)
                return false;

            float baseAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

            float bestAngle = baseAngle;
            float bestError = Mathf.Infinity;

            int numOfSteps = Mathf.CeilToInt(maxTime / simulationStep);

            for (int i = 0; i < 180; i++) {
                float angle = baseAngle + (i % 2 == 0 ? i / 2 : -(i + 1) / 2);
                float radians = angle * Mathf.Deg2Rad;

                Vector2 aimDirection = new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

                Vector2 initialVelocity = aimDirection * speed;

                MoveTransformSimulator simulator = new MoveTransformSimulator(
                    start,
                    initialVelocity,
                    gravity,
                    drag,
                    dragMinSpeed,
                    multiplier
                );

                Vector2 previousPosition = simulator.Position;

                for (int step = 0; step < numOfSteps; step++) {
                    simulator.Step(simulationStep);

                    float error = DistanceToSegmentSquared(
                        target,
                        previousPosition,
                        simulator.Position
                    );

                    if (error < bestError) {
                        bestError = error;
                        bestAngle = angle;
                    }

                    previousPosition = simulator.Position;
                }

                if (bestError < 0.25f) break;
            }

            if (bestError >= 0.25f)
                return false;

            float bestRadians = bestAngle * Mathf.Deg2Rad;

            direction = new Vector2(
                Mathf.Cos(bestRadians),
                Mathf.Sin(bestRadians)
            );

            return true;
        }

        private static float DistanceToSegmentSquared(
    Vector2 point,
    Vector2 segmentStart,
    Vector2 segmentEnd
) {
            Vector2 segment = segmentEnd - segmentStart;

            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared < 0.000001f)
                return (point - segmentStart).sqrMagnitude;

            float t = Vector2.Dot(point - segmentStart, segment) / lengthSquared;
            t = Mathf.Clamp01(t);

            Vector2 closestPoint = segmentStart + segment * t;

            return (point - closestPoint).sqrMagnitude;
        }
    }
}
