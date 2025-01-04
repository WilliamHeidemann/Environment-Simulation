using Jobs;
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
        var agentSpeeds = new NativeArray<float>(agents.Count, Allocator.TempJob);
        var agentPositions = new NativeArray<Vector3>(agents.Count, Allocator.TempJob);
        var agentRotations = new NativeArray<Quaternion>(agents.Count, Allocator.TempJob);

        for (int i = 0; i < agents.Count; i++)
        {
            targetPositions[i] = agents[i].TargetPosition;
            agentSpeeds[i] = agents[i].Speed;
            agentPositions[i] = agents[i].Position;
            agentRotations[i] = agents[i].Rotation;
        }

        var job = new TranslateJob
        {
            TargetPositions = targetPositions,
            AgentSpeeds = agentSpeeds,
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
        agentSpeeds.Dispose();
        agentPositions.Dispose();
        agentRotations.Dispose();
    }
}