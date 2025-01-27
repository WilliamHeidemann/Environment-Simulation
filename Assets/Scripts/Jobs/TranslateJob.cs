using DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Jobs
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
    public struct TranslateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<AgentMotion> AgentMotions;
        public NativeArray<AgentTransform> AgentTransforms;
        [ReadOnly] public float DeltaTime;
        
        public void Execute(int index)
        {
            if (math.lengthsq(AgentMotions[index].Velocity) == 0f)
            {
                return;
            }
            
            float3 position = AgentTransforms[index].Position + math.normalize(AgentMotions[index].Velocity) * AgentMotions[index].Speed * DeltaTime;
            quaternion rotation = quaternion.LookRotation(AgentMotions[index].Velocity, math.up());
            
            AgentTransforms[index] = new AgentTransform
            {
                Position = position,
                Rotation = rotation
            };
        }
    }
}