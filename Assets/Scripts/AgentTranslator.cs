using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AgentTranslator : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;

    private void Update()
    {
        var agents = _agentsData.Agents;
        var targetPositions = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
        var agentPositions = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
        var agentRotations = new NativeArray<Quaternion>(agents.Count, Allocator.TempJob);

        for (int i = 0; i < agents.Count; i++)
        {
            targetPositions[i] = agents[i].TargetPosition;
            agentPositions[i] = agents[i].Position;
            agentRotations[i] = agents[i].Rotation;
        }

        var job = new TranslateJob
        {
            TargetPositions = targetPositions,
            AgentPositions = agentPositions,
            AgentRotations = agentRotations,
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
    }
}

public struct TranslateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> TargetPositions;
    [ReadOnly] public float DeltaTime;
    public NativeArray<Vector3> AgentPositions;
    public NativeArray<Quaternion> AgentRotations;

    private const float RotationSpeed = 150f;
    private const float TranslationSpeed = 0.15f;

    public void Execute(int index)
    {
        Vector3 direction = TargetPositions[index] - AgentPositions[index];
        if (direction.sqrMagnitude < 0.1f)
        {
            return;
        }

        Rotate(index, direction);
        Translate(index, direction);
    }

    private void Rotate(int index, Vector3 direction)
    {
        Quaternion destinationRotation = Quaternion.LookRotation(direction);
        float angle = Quaternion.Angle(AgentRotations[index], destinationRotation);

        if (angle > 1f)
        {
            AgentRotations[index] =
                Quaternion.RotateTowards(AgentRotations[index], destinationRotation, RotationSpeed * DeltaTime);
        }
    }

    private void Translate(int index, Vector3 direction)
    {
        Vector3 distanceToMove = direction * (TranslationSpeed * DeltaTime);
        AgentPositions[index] += distanceToMove;
    }
}