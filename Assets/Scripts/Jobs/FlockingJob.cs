using DataStructures;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct FlockingJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Vector3> Acceleration;
        [WriteOnly] public NativeArray<float> Speed;

        [ReadOnly] public NativeArray<Vector3> Positions;

        [ReadOnly] public NativeArray<AgentTransform> Close;
        [ReadOnly] public NativeArray<int> Offset;
        [ReadOnly] public NativeArray<int> Lengths;

        [ReadOnly] public float CohesionStrength;
        [ReadOnly] public float AlignmentStrength;
        [ReadOnly] public float SeparationStrength;
        [ReadOnly] public float DeltaTime;

        [WriteOnly] public NativeArray<Vector3> DebugCentreOfFlock;
        [WriteOnly] public NativeArray<Vector3> DebugAlignment;
        [WriteOnly] public NativeArray<Vector3> DebugSeparation;

        private const float Fast = 3f;
        private const float Medium = 1f;
        private const float Slow = .1f;

        private const int FewNearbySheep = 12;
        private const int ManyNearbySheep = 30;

        private const float CohesionSquareThreshold = 25f;
        private const float SeparationSquareThreshold = 2f;

        public void Execute(int index)
        {
            Vector3 centreOfFlock = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 separation = Vector3.zero;

            int start = Offset[index];
            int end = start + Lengths[index];
            int closeCount = 0;
            for (var i = start; i < end; i++)
            {
                var edge = new Edge
                {
                    SquareDistance = Vector3.SqrMagnitude(Positions[index] - Close[i].Position),
                    SeparationVector = Positions[index] - Close[i].Position,
                    EndPosition = Close[i].Position,
                    EndForward = Close[i].Rotation * Vector3.forward,
                };
                
                if (edge.SquareDistance is > CohesionSquareThreshold or 0f) continue;

                closeCount++;
                centreOfFlock += edge.EndPosition;
                alignment += edge.EndForward;

                if (edge.SquareDistance is > SeparationSquareThreshold or 0f) continue;
                separation += edge.SeparationVector / edge.SquareDistance;
            }

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
                // closeCount = 1;
            }

            float closeCountInverse = 1f / closeCount;
            centreOfFlock *= closeCountInverse;
            centreOfFlock -= Positions[index];
            centreOfFlock *= CohesionStrength;
            alignment *= closeCountInverse;
            alignment *= AlignmentStrength;
            separation *= SeparationStrength;

            DebugCentreOfFlock[index] = centreOfFlock;
            DebugAlignment[index] = alignment;
            DebugSeparation[index] = separation;
            Vector3 acceleration = (alignment + centreOfFlock + separation) * DeltaTime;
            acceleration = Vector3.ClampMagnitude(acceleration, 3f);
            Acceleration[index] = acceleration;
        }
    }
}