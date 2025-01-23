using DataStructures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
            if (AgentMotions[index].Velocity == Vector3.zero)
            {
                return;
            }
            
            Vector3 position = AgentTransforms[index].Position + AgentMotions[index].Velocity.normalized * AgentMotions[index].Speed * DeltaTime;
            Quaternion rotation = Quaternion.LookRotation(AgentMotions[index].Velocity);
            
            AgentTransforms[index] = new AgentTransform
            {
                Position = position,
                Rotation = rotation
            };
        }
    }
}