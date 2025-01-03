using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UtilityToolkit.Runtime;
using Random = UnityEngine.Random;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    [SerializeField] private AgentsData AgentsData;

    public void Update()
    {
        #region OldGraphConstruction
        // const float closeDistance = 25f;
        // const float tooCloseDistance = 2f;
        //
        // for (int i = 0; i < AgentsData.Agents.Count; i++)
        // {
        //     for (int j = i + 1; j < AgentsData.Agents.Count; j++)
        //     {
        //         Agent agent = AgentsData.Agents[i];
        //         Agent other = AgentsData.Agents[j];
        //         if (agent == other) continue;
        //
        //         Vector3 separationVector = agent.Position - other.Position;
        //         float squareDistance = separationVector.sqrMagnitude;
        //
        //         Edge edge1 = new()
        //         {
        //             SquareDistance = squareDistance,
        //             SeparationVector = separationVector,
        //             EndPosition = other.Position,
        //             Index = other.Index
        //         };
        //
        //         Edge edge2 = new()
        //         {
        //             SquareDistance = squareDistance,
        //             SeparationVector = other.Position - agent.Position,
        //             EndPosition = agent.Position,
        //             Index = agent.Index
        //         };
        //
        //         if (squareDistance < closeDistance)
        //         {
        //             AgentsData.ProximityGraph.Close[agent].Add(edge1);
        //             AgentsData.ProximityGraph.Close[other].Add(edge2);
        //         }
        //         else
        //         {
        //             AgentsData.ProximityGraph.Close[agent].Remove(edge1);
        //             AgentsData.ProximityGraph.Close[other].Remove(edge2);
        //         }
        //
        //         if (squareDistance < tooCloseDistance)
        //         {
        //             AgentsData.ProximityGraph.TooClose[agent].Add(edge1);
        //             AgentsData.ProximityGraph.TooClose[other].Add(edge2);
        //         }
        //         else
        //         {
        //             AgentsData.ProximityGraph.TooClose[agent].Remove(edge1);
        //             AgentsData.ProximityGraph.TooClose[other].Remove(edge2);
        //         }
        //     }
        // }
        #endregion

        int agentCount = AgentsData.Agents.Count;
        var agentPositions = new NativeArray<Vector3>(agentCount, Allocator.TempJob);
        for (int i = 0; i < agentCount; i++)
        {
            agentPositions[i] = AgentsData.Agents[i].Position;
        }

        var jobHandles = new NativeArray<JobHandle>(agentCount, Allocator.TempJob);
        var jobs = new ConstructGraphJob[agentCount];

        for (int i = 0; i < agentCount; i++)
        {
            var close = new NativeArray<Edge>(agentCount, Allocator.TempJob);
            var tooClose = new NativeArray<Edge>(agentCount, Allocator.TempJob);
            var constructGraphJob = new ConstructGraphJob
            {
                AgentPositions = agentPositions,
                Close = close,
                TooClose = tooClose,
                AgentIndex = i
            };
            jobs[i] = constructGraphJob;
            JobHandle job = constructGraphJob.Schedule(agentCount, 128);
            jobHandles[i] = job;
        }

        JobHandle.CompleteAll(jobHandles);
        
        for (int i = 0; i < agentCount; i++)
        {
            ConstructGraphJob job = jobs[i];
            var close = job.Close.Where(edge => edge != default).ToHashSet();
            var tooClose = job.TooClose.Where(edge => edge != default).ToHashSet();
            AgentsData.ProximityGraph.Close[AgentsData.Agents[i]] = new HashSet<Edge>(close);
            AgentsData.ProximityGraph.TooClose[AgentsData.Agents[i]] = new HashSet<Edge>(tooClose);
            job.Close.Dispose();
            job.TooClose.Dispose();
        }

        agentPositions.Dispose();
        jobHandles.Dispose();

        foreach (Agent agent in AgentsData.Agents)
        {
            switch (agent.Behavior)
            {
                case Behavior.Wandering:
                    UpdateWanderingTarget(agent);
                    break;
                case Behavior.Flocking:
                    UpdateFlockingTarget(agent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    private void UpdateWanderingTarget(Agent agent)
    {
        if (Vector3.SqrMagnitude(agent.TargetPosition - agent.Position) < 1f)
        {
            agent.TargetPosition = Utility.RandomOnCircle(10);
        }
    }

    private void UpdateFlockingTarget(Agent agent)
    {
        int cohesionCount = AgentsData.ProximityGraph.Close[agent].Count;
        if (cohesionCount == 0)
        {
            return;
        }

        Vector3 cohesion = Vector3.zero;
        foreach (Edge edge in AgentsData.ProximityGraph.Close[agent])
        {
            cohesion += edge.EndPosition;
        }

        cohesion /= cohesionCount;
        cohesion -= agent.Position;

        Vector3 separation = Vector3.zero;
        foreach (Edge edge in AgentsData.ProximityGraph.TooClose[agent])
        {
            separation += (edge.SeparationVector.normalized / edge.SquareDistance) * 5f;
        }

        Vector3 acceleration = cohesion + separation;

        if (cohesion.sqrMagnitude < 2f)
        {
            acceleration = Vector3.zero;
        }
        else if (cohesion.sqrMagnitude < 4f)
        {
            acceleration = agent.Rotation * Vector3.forward;
        }

        agent.TargetPosition = agent.Position + acceleration;
    }

    private void OnDrawGizmos()
    {
        if (AgentsData.Agents == null) return;
        Gizmos.color = Color.red;
        foreach (Agent agent in AgentsData.Agents)
        {
            Gizmos.DrawLine(agent.Position, agent.TargetPosition);
            Gizmos.DrawCube(agent.TargetPosition, Vector3.one * 1f);
        }
    }
}

public struct ConstructGraphJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> AgentPositions;
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
                Index = index
            };
            Close[index] = edge;

            if (squareDistance < TooCloseDistance)
            {
                TooClose[index] = edge;
            }
        }
    }
}