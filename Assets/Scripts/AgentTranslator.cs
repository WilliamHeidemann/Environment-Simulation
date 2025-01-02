using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityToolkit.Runtime;

public class AgentTranslator : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;

    private void Update()
    {
        var agents = _agentsData.Agents;
        var targetPositions = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
        var agentPositions = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
        var agentRotations = new NativeArray<Quaternion>(agents.Count, Allocator.TempJob);
        var haveTargetPositions = new NativeArray<bool>(agents.Count, Allocator.TempJob);

        for (int i = 0; i < agents.Count; i++)
        {
            targetPositions[i] = agents[i].TargetPosition.Match(
                some: position => position,
                none: agents[i].Position);
            agentPositions[i] = agents[i].Position;
            agentRotations[i] = agents[i].Rotation;
            haveTargetPositions[i] = agents[i].TargetPosition.IsSome(out _);
        }
        
        var job = new TranslateJob
        {
            TargetPositions = targetPositions,
            AgentPositions = agentPositions,
            AgentRotations = agentRotations,
            HaveTargetPositions = haveTargetPositions,
            DeltaTime = Time.deltaTime
        };
        
        job.Schedule(agents.Count, 1).Complete();
        
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].Position = agentPositions[i];
            agents[i].Rotation = agentRotations[i];
        }
        
        targetPositions.Dispose();
        agentPositions.Dispose();
        agentRotations.Dispose();
        haveTargetPositions.Dispose();
    }
}

public class Agent
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Option<Vector3> TargetPosition;
    public float StartIdleTime;
}

public struct TranslateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> TargetPositions;
    [ReadOnly] public NativeArray<bool> HaveTargetPositions;
    [ReadOnly] public float DeltaTime;
    public NativeArray<Vector3> AgentPositions;
    public NativeArray<Quaternion> AgentRotations;
    
    private const float RotationSpeed = 200f;
    private const float TranslationSpeed = 2f;
    
    public void Execute(int index)
    {
        if (!HaveTargetPositions[index])
        {
            return;
        }
        Rotate(index);
        Translate(index);
    }
    
    private void Rotate(int index)
    {
        float angle = Quaternion.Angle(
            AgentRotations[index],
            Quaternion.LookRotation(AgentPositions[index] - TargetPositions[index]));

        if (angle > 1f)
        {
            AgentRotations[index] = Quaternion.RotateTowards(
                AgentRotations[index],
                Quaternion.LookRotation(AgentPositions[index] - TargetPositions[index]),
                RotationSpeed * DeltaTime);
        }
    }
    
    private void Translate(int index)
    {
        Vector3 direction = TargetPositions[index] - AgentPositions[index];
        Vector3 distanceToMove = direction.normalized * (TranslationSpeed * DeltaTime);
        AgentPositions[index] += distanceToMove;
    }
}