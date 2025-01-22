using DataStructures;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct FlockingJob2 : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Vector3> Acceleration;
        [WriteOnly] public NativeArray<float> Speed;
        
        [ReadOnly] public NativeArray<Edge> Close;
        [ReadOnly] public NativeArray<int> Offset;
        [ReadOnly] public NativeArray<int> Lengths;
        
        [ReadOnly] public float CohesionStrength;
        [ReadOnly] public float AlignmentStrength;
        [ReadOnly] public float SeparationStrength;
        [ReadOnly] public float DeltaTime;

        public void Execute(int index)
        {
            int closeCount = Lengths[index];

            if (closeCount == 0)
            {
                return;
            }

            Speed[index] = closeCount switch
            {
                < 12 => 8f,
                < 30 => 3f,
                _ => 1.5f
            };

            Vector3 cohesion = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 separation = Vector3.zero;

            int start = Offset[index];
            int end = start + Lengths[index];
            for (var i = start; i < end; i++)
            {
                Edge edge = Close[i];
                if (edge.SquareDistance is 0f or > 25f) continue;

                cohesion += edge.EndPosition;
                alignment += edge.EndForward;

                if (edge.SquareDistance > 2f) continue;
                separation += edge.SeparationVector.normalized / edge.SquareDistance;
            }

            float closeCountInverse = 1f / closeCount;
            cohesion *= closeCountInverse;
            // cohesion -= Positions[index];
            cohesion *= CohesionStrength;
            alignment *= closeCountInverse;
            alignment *= AlignmentStrength;
            separation *= SeparationStrength;

            Vector3 acceleration = (alignment + cohesion + separation) * DeltaTime;
            Acceleration[index] = acceleration;
        }
    }
}