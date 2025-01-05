using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
    public struct FlockingJob : IJob
    {
        public NativeArray<Vector3> Acceleration;
        public NativeArray<float> Speed;
        [ReadOnly] public Vector3 Position;
        [ReadOnly] public NativeArray<Edge> Close;
        [ReadOnly] public NativeArray<Edge> TooClose;
        [ReadOnly] public float CohesionStrength;
        [ReadOnly] public float AlignmentStrength;
        [ReadOnly] public float SeparationStrength;

        public void Execute()
        {
            int closeCount = 0;
            for (int i = 0; i < Close.Length; i++)
            {
                closeCount += Close[i] != default ? 1 : 0;
            }
            if (closeCount == 0)
            {
                return;
            }

            Speed[0] = closeCount switch
            {
                < 12 => 8f,
                < 30 => 3f,
                _ => 1.5f
            };

            Vector3 cohesion = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            for (var i = 0; i < Close.Length; i++)
            {
                Edge edge = Close[i];
                if (edge == default) continue;
                cohesion += edge.EndPosition;
                alignment += edge.EndForward;
            }

            float closeCountInverse = 1f / closeCount;
            cohesion *= closeCountInverse;
            cohesion -= Position;
            cohesion *= CohesionStrength;
            alignment *= closeCountInverse;
            alignment *= AlignmentStrength;

            Vector3 separation = Vector3.zero;
            for (var i = 0; i < TooClose.Length; i++)
            {
                Edge edge = TooClose[i];
                if (edge == default) continue;
                separation += edge.SeparationVector.normalized / edge.SquareDistance;
            }

            separation *= SeparationStrength;

            Vector3 acceleration = alignment + cohesion + separation;
            Acceleration[0] = acceleration;
        }
    }
}