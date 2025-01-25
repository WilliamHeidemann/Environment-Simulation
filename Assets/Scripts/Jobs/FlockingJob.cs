using System.Collections.Generic;
using DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
    public struct FlockingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<AgentTransform> Transforms;
        public NativeArray<AgentMotion> Motions;

        [ReadOnly] public NativeSpatialHashGrid SpatialHashGrid;

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

            int closeCount = 0;
            NativeList<AgentTransform> neighbors = new NativeList<AgentTransform>(100, Allocator.TempJob);
            SpatialHashGrid.QueryNeighbors(Transforms[index].Position, ref neighbors);
            for (var i = 0; i < neighbors.Length; i++)
            {
                AgentTransform nearbyAgent = neighbors[i];
                var edge = new Edge
                {
                    SquareDistance = Vector3.SqrMagnitude(Transforms[index].Position - nearbyAgent.Position),
                    SeparationVector = Transforms[index].Position - nearbyAgent.Position,
                    EndPosition = nearbyAgent.Position,
                    EndForward = nearbyAgent.Rotation * Vector3.forward,
                };

                if (edge.SquareDistance is > CohesionSquareThreshold or 0f) continue;
                closeCount++;
                centreOfFlock += edge.EndPosition;
                alignment += edge.EndForward;

                if (edge.SquareDistance > SeparationSquareThreshold) continue;
                separation += edge.SeparationVector / edge.SquareDistance;
            }

            neighbors.Dispose();

            float speed = closeCount switch
            {
                < FewNearbySheep => Fast,
                < ManyNearbySheep => Medium,
                // else, very large crowd
                _ => Slow
            };

            if (closeCount == 0)
            {
                return;
            }

            float closeCountInverse = 1f / closeCount;
            centreOfFlock *= closeCountInverse;
            centreOfFlock -= Transforms[index].Position;
            centreOfFlock *= CohesionStrength;
            alignment *= closeCountInverse;
            alignment *= AlignmentStrength;
            separation *= SeparationStrength;

            DebugCentreOfFlock[index] = centreOfFlock;
            DebugAlignment[index] = alignment;
            DebugSeparation[index] = separation;
            Vector3 acceleration = (alignment + centreOfFlock + separation) * DeltaTime;
            acceleration = Vector3.ClampMagnitude(acceleration, 3f);
            Vector3 velocity = Motions[index].Velocity + acceleration;
            velocity = Vector3.ClampMagnitude(velocity, 5f);

            Motions[index] = new AgentMotion
            {
                Speed = speed,
                Velocity = velocity
            };
        }
    }
}