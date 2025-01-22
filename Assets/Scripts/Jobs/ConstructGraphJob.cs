using DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
    public struct ConstructGraphJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> AgentPositions;
        [ReadOnly] public NativeArray<Vector3> AgentForwards;
        public NativeArray<Edge> Close;
        public NativeArray<Edge> TooClose;
        public int AgentIndex;

        private const float CloseDistance = 25f;
        private const float TooCloseDistance = 2f;

        public void Execute(int index)
        {
            if (AgentIndex == index)
            {
                return;
            }

            Vector3 separationVector = AgentPositions[AgentIndex] - AgentPositions[index];
            float squareDistance = separationVector.sqrMagnitude;

            if (squareDistance < CloseDistance)
            {
                Edge edge = new()
                {
                    SquareDistance = squareDistance,
                    SeparationVector = separationVector,
                    EndPosition = AgentPositions[index],
                    EndForward = AgentForwards[index],
                    // Index = index
                };
                Close[index] = edge;

                if (squareDistance < TooCloseDistance)
                {
                    TooClose[index] = edge;
                }
            }
            else
            {
                Close[index] = default;
                TooClose[index] = default;
            }
        }
    }
}