using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityToolkit.Runtime;
using Random = UnityEngine.Random;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    [SerializeField] private AgentsData AgentsData;

    public void Update()
    {
        const float closeDistance = 25f;
        const float tooCloseDistance = 2f;

        for (int i = 0; i < AgentsData.Agents.Count; i++)
        {
            for (int j = i + 1; j < AgentsData.Agents.Count; j++)
            {
                Agent agent = AgentsData.Agents[i];
                Agent other = AgentsData.Agents[j];
                if (agent == other) continue;

                Vector3 separationVector = agent.Position - other.Position;
                float squareDistance = separationVector.sqrMagnitude;

                Edge edge1 = new()
                {
                    SquareDistance = squareDistance,
                    SeparationVector = separationVector,
                    EndPosition = other.Position,
                    Index = other.Index
                };

                Edge edge2 = new()
                {
                    SquareDistance = squareDistance,
                    SeparationVector = other.Position - agent.Position,
                    EndPosition = agent.Position,
                    Index = agent.Index
                };

                if (squareDistance < closeDistance)
                {
                    AgentsData.ProximityGraph.Close[agent].Add(edge1);
                    AgentsData.ProximityGraph.Close[other].Add(edge2);
                }
                else
                {
                    AgentsData.ProximityGraph.Close[agent].Remove(edge1);
                    AgentsData.ProximityGraph.Close[other].Remove(edge2);
                }

                if (squareDistance < tooCloseDistance)
                {
                    AgentsData.ProximityGraph.TooClose[agent].Add(edge1);
                    AgentsData.ProximityGraph.TooClose[other].Add(edge2);
                }
                else
                {
                    AgentsData.ProximityGraph.TooClose[agent].Remove(edge1);
                    AgentsData.ProximityGraph.TooClose[other].Remove(edge2);
                }
            }
        }

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