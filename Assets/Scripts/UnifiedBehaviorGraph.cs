using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UtilityToolkit.Runtime;
using Random = UnityEngine.Random;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    [SerializeField] private AgentsData AgentsData;
    [SerializeField] private float CohesionStrength;
    [SerializeField] private float AlignmentStrength;
    [SerializeField] private float SeparationStrength;

    public void Update()
    {
        int agentCount = AgentsData.Agents.Count;
        var agentPositions = new NativeArray<Vector3>(agentCount, Allocator.TempJob);
        var agentForwards = new NativeArray<Vector3>(agentCount, Allocator.TempJob);
        for (int i = 0; i < agentCount; i++)
        {
            agentPositions[i] = AgentsData.Agents[i].Position;
            agentForwards[i] = AgentsData.Agents[i].Rotation * Vector3.forward;
        }

        var graphJobHandles = new NativeArray<JobHandle>(agentCount, Allocator.TempJob);
        var graphJobs = new ConstructGraphJob[agentCount];
        var flockingJobHandles = new NativeArray<JobHandle>(agentCount, Allocator.TempJob);
        var flockingJobs = new FlockingJob[agentCount];

        for (int i = 0; i < agentCount; i++)
        {
            var close = new NativeArray<Edge>(agentCount, Allocator.TempJob);
            var tooClose = new NativeArray<Edge>(agentCount, Allocator.TempJob);
            var graphJob = new ConstructGraphJob
            {
                AgentPositions = agentPositions,
                AgentForwards = agentForwards,
                Close = close,
                TooClose = tooClose,
                AgentIndex = i
            };
            graphJobs[i] = graphJob;
            JobHandle graphJobHandle = graphJob.Schedule(agentCount, 128);
            graphJobHandles[i] = graphJobHandle;
            
            Agent agent = AgentsData.Agents[i];
            var acceleration = new NativeArray<Vector3>(1, Allocator.TempJob);
            var speed = new NativeArray<float>(1, Allocator.TempJob);
            var flockingJob = new FlockingJob
            {
                Acceleration = acceleration,
                Speed = speed,
                Position = agent.Position,
                Close = close,
                TooClose = tooClose,
                CohesionStrength = CohesionStrength,
                AlignmentStrength = AlignmentStrength,
                SeparationStrength = SeparationStrength
            };
            flockingJobs[i] = flockingJob;
            JobHandle flockingJobHandle = flockingJob.Schedule(graphJobHandle);
            flockingJobHandles[i] = flockingJobHandle;
        }

        JobHandle finalJobHandle = JobHandle.CombineDependencies(flockingJobHandles);
        finalJobHandle.Complete();

        agentPositions.Dispose();
        agentForwards.Dispose();
        graphJobHandles.Dispose();
        
        for (int i = 0; i < agentCount; i++)
        {
            FlockingJob job = flockingJobs[i];
            Agent agent = AgentsData.Agents[i];
            agent.TargetPosition = agent.Position + job.Acceleration[0];
            agent.Speed = job.Speed[0];
            job.Acceleration.Dispose();
            job.Speed.Dispose();
            job.Close.Dispose();
            job.TooClose.Dispose();
        }
        
        flockingJobHandles.Dispose();
    }
    
    private void OnDrawGizmos()
    {
        if (AgentsData.Agents == null) return;
        foreach (Agent agent in AgentsData.Agents)
        {
            Gizmos.color = agent.DebugCohesion switch
            {
                DebugCohesion.NoCohesion => Color.black,
                DebugCohesion.LowCohesion => Color.red, 
                DebugCohesion.MiddleCohesion => Color.yellow,
                DebugCohesion.HighCohesion => Color.green,
                _ => throw new ArgumentOutOfRangeException()
            };
            Gizmos.DrawLine(agent.Position, agent.TargetPosition);
            Gizmos.DrawCube(agent.TargetPosition, Vector3.one * 1f);
        }
    }
}