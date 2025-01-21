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

    [SerializeField] private Transform Shepherd;

    private NativeArray<Vector3> _agentPositions;
    private NativeArray<Vector3> _agentForwards;
    private ConstructGraphJob[] _constructGraphJobs;
    
    private NativeArray<JobHandle> _flockingJobHandles;
    private FlockingJob[] _flockingJobs;

    private void Start()
    {
        int count = AgentsData.Agents.Count;
        _agentPositions = new NativeArray<Vector3>(count, Allocator.Persistent);
        _agentForwards = new NativeArray<Vector3>(count, Allocator.Persistent);
        _constructGraphJobs = new ConstructGraphJob[count];
        for (int i = 0; i < count; i++)
        {
            _constructGraphJobs[i] = new ConstructGraphJob
            {
                AgentPositions = _agentPositions,
                AgentForwards = _agentForwards,
                Close = new NativeArray<Edge>(count, Allocator.Persistent),
                TooClose = new NativeArray<Edge>(count, Allocator.Persistent),
                AgentIndex = i
            };
        }
        
        _flockingJobHandles = new NativeArray<JobHandle>(count, Allocator.Persistent);
        _flockingJobs = new FlockingJob[count];
        for (int i = 0; i < count; i++)
        {
            _flockingJobs[i] = new FlockingJob
            {
                Acceleration = new NativeArray<Vector3>(1, Allocator.Persistent),
                Speed = new NativeArray<float>(1, Allocator.Persistent),
                // Position = AgentsData.Agents[i].Position,
                Close = _constructGraphJobs[i].Close,
                TooClose = _constructGraphJobs[i].TooClose,
                CohesionStrength = CohesionStrength,
                AlignmentStrength = AlignmentStrength,
                SeparationStrength = SeparationStrength
            };
        }
    }

    public void Update()
    {
        int agentCount = AgentsData.Agents.Count;
        for (int i = 0; i < agentCount; i++)
        {
            _agentPositions[i] = AgentsData.Agents[i].Position;
            _agentForwards[i] = AgentsData.Agents[i].Rotation * Vector3.forward;
        }

        for (int i = 0; i < agentCount; i++)
        {
            JobHandle graphJobHandle = _constructGraphJobs[i].Schedule(agentCount, 128);
            _flockingJobs[i].Position = AgentsData.Agents[i].Position;
            _flockingJobHandles[i] = _flockingJobs[i].Schedule(graphJobHandle);
        }

        JobHandle.CombineDependencies(_flockingJobHandles).Complete();
        
        for (int i = 0; i < agentCount; i++)
        {
            FlockingJob job = _flockingJobs[i];
            Agent agent = AgentsData.Agents[i];
            agent.TargetPosition = agent.Position + job.Acceleration[0];
            agent.Speed = job.Speed[0];
        }

        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            float distance = Vector3.SqrMagnitude(agent.Position - Shepherd.position);
            if (distance > 100f) continue;
            Vector3 direction = agent.Position - Shepherd.position;
            float strength = -(distance * 0.1f) + 20f;
            // Vector3 separation = agent.Position - agent.TargetPosition;
            agent.TargetPosition += direction * (strength  * 0.01f);
            agent.Speed += strength;
        }
    }

    private void OnDestroy()
    {
        _agentPositions.Dispose();
        _agentForwards.Dispose();
        foreach (FlockingJob job in _flockingJobs)
        {
            job.Acceleration.Dispose();
            job.Speed.Dispose();
        }
        foreach (ConstructGraphJob job in _constructGraphJobs)
        {
            job.Close.Dispose();
            job.TooClose.Dispose();
        }
        _flockingJobHandles.Dispose();
    }

    private void OnDrawGizmos()
    {
        // if (AgentsData.Agents == null) return;
        // foreach (Agent agent in AgentsData.Agents)
        // {
        //     Gizmos.color = agent.DebugCohesion switch
        //     {
        //         DebugCohesion.NoCohesion => Color.black,
        //         DebugCohesion.LowCohesion => Color.red, 
        //         DebugCohesion.MiddleCohesion => Color.yellow,
        //         DebugCohesion.HighCohesion => Color.green,
        //         _ => throw new ArgumentOutOfRangeException()
        //     };
        //     Gizmos.DrawLine(agent.Position, agent.TargetPosition);
        //     Gizmos.DrawCube(agent.TargetPosition, Vector3.one * 1f);
        // }
        // foreach (Agent agent in AgentsData.Agents)
        // {
        //     Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        //     Gizmos.DrawWireSphere(agent.Position, 5f);
        // }
    }
}