using System.Collections.Generic;
using DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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

        // [WriteOnly] public NativeArray<Vector3> DebugCentreOfFlock;
        // [WriteOnly] public NativeArray<Vector3> DebugAlignment;
        // [WriteOnly] public NativeArray<Vector3> DebugSeparation;

        private const float Fast = 3f;
        private const float Medium = 1f;
        private const float Slow = .1f;

        private const int FewNearbySheep = 12;
        private const int ManyNearbySheep = 30;

        private const float CohesionSquareThreshold = 25f;
        private const float SeparationSquareThreshold = 2f;

        public void Execute(int index)
        {
            float3 centreOfFlock = float3.zero;
            float3 alignment = float3.zero;
            float3 separation = float3.zero;

            int closeCount = 0;
            NativeList<AgentTransform> neighbors = new NativeList<AgentTransform>(100, Allocator.TempJob);
            SpatialHashGrid.QueryNeighbors(Transforms[index].Position, ref neighbors);
            for (var i = 0; i < neighbors.Length; i++)
            {
                AgentTransform nearbyAgent = neighbors[i];
                var edge = new Edge
                {
                    SquareDistance = math.lengthsq(Transforms[index].Position - nearbyAgent.Position),
                    SeparationVector = Transforms[index].Position - nearbyAgent.Position,
                    EndPosition = nearbyAgent.Position,
                    EndForward = math.mul(nearbyAgent.Rotation, new float3(0, 0, 1))
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

            // DebugCentreOfFlock[index] = centreOfFlock;
            // DebugAlignment[index] = alignment;
            // DebugSeparation[index] = separation;
            float3 acceleration = (alignment + centreOfFlock + separation) * DeltaTime;
            float squaredAccelerationMagnitude = math.lengthsq(acceleration);
            if (squaredAccelerationMagnitude > 9f)
            {
                acceleration = math.normalize(acceleration) * 3f;
            }
            float3 velocity = Motions[index].Velocity + acceleration;
            float squaredVelocityMagnitude = math.lengthsq(velocity);
            if (squaredVelocityMagnitude > 25f)
            {
                velocity = math.normalize(velocity) * 5f;
            }

            Motions[index] = new AgentMotion
            {
                Speed = speed,
                Velocity = velocity
            };
        }
    }
}