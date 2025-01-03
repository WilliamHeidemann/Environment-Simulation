using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityToolkit.Runtime;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    [SerializeField] private AgentsData AgentsData;

    public void Update()
    {
        const float closeDistance = 5f;
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
                    Other = other,
                    SquareDistance = squareDistance,
                    SeparationVector = separationVector
                };
                
                Edge edge2 = new()
                {
                    Other = agent,
                    SquareDistance = squareDistance,
                    SeparationVector = other.Position - agent.Position
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
            Vector3 acceleration = Vector3.zero;
            
            Vector3 cohesion = Vector3.zero;
            foreach (Edge edge in AgentsData.ProximityGraph.Close[agent])
            {
                cohesion += edge.Other.Position;
            }
            
            int cohesionCount = AgentsData.ProximityGraph.Close[agent].Count;
            if (cohesionCount == 0) continue;
            cohesion /= cohesionCount;
            cohesion -= agent.Position;
            
            Vector3 separation = Vector3.zero;
            foreach (Edge edge in AgentsData.ProximityGraph.TooClose[agent])
            {
                separation += edge.SeparationVector / edge.SquareDistance;
            }
            
            acceleration += cohesion;
            acceleration += separation * 3f;
            
            agent.TargetPosition += acceleration;
            // Vector3 offsetTargetPosition = agent.TargetPosition - agent.Position;
            // offsetTargetPosition = Vector3.ClampMagnitude(offsetTargetPosition, 20f);
            // agent.TargetPosition = agent.Position + offsetTargetPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if (AgentsData.Agents == null) return;
        Gizmos.color = Color.red;
        foreach (Agent agent in AgentsData.Agents)
        {
            Gizmos.DrawCube(agent.TargetPosition, Vector3.one * 1f);
        }
    }
}