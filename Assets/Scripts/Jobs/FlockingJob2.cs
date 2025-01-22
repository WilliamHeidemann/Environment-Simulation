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
        
        [ReadOnly] public NativeArray<Vector3> Positions;

        [ReadOnly] public NativeArray<Edge> Close;
        [ReadOnly] public NativeArray<int> Offset;
        [ReadOnly] public NativeArray<int> Lengths;

        [ReadOnly] public float CohesionStrength;
        [ReadOnly] public float AlignmentStrength;
        [ReadOnly] public float SeparationStrength;
        [ReadOnly] public float DeltaTime;

        private const float Fast = 3f;
        private const float Medium = 1f;
        private const float Slow = .1f;
        
        private const int FewNearbySheep = 12;
        private const int ManyNearbySheep = 30;
        
        private const float CohesionSquareThreshold = 25f;
        private const float SeparationSquareThreshold = 2f;
        
        public void Execute(int index)
        {
            int closeCount = Lengths[index];
            
            Speed[index] = closeCount switch
            {
                < FewNearbySheep => Fast,
                < ManyNearbySheep => Medium,
                // else, very large crowd
                _ => Slow
            };
            
            if (closeCount == 0)
            {
                Acceleration[index] = Vector3.zero;
                return;
            }

            Vector3 cohesion = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 separation = Vector3.zero;

            int start = Offset[index];
            int end = start + Lengths[index];
            for (var i = start; i < end; i++)
            {
                Edge edge = Close[i];
                if (edge.SquareDistance is > CohesionSquareThreshold or 0f) continue;

                // cohesion += edge.EndPosition;
                cohesion += edge.SeparationVector;
                alignment += edge.EndForward;

                if (edge.SquareDistance is > SeparationSquareThreshold or 0f) continue;
                separation += edge.SeparationVector / edge.SquareDistance;
            }

            float closeCountInverse = 1f / closeCount;
            cohesion *= closeCountInverse;
            cohesion *= CohesionStrength;
            alignment *= closeCountInverse;
            alignment *= AlignmentStrength;
            separation *= SeparationStrength;

            Vector3 acceleration = (alignment + cohesion + separation) * DeltaTime;
            Acceleration[index] = acceleration;
        }
    }
}