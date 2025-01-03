using System.Collections.Generic;
using UnityEngine;
using UtilityToolkit.Runtime;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private AgentRenderer _agentRenderer;

    private void Start()
    {
        _agentsData.Agents = new List<Agent>();
        _agentsData.ProximityGraph = new ProximityGraph();
        Random.InitState(0);
        for (int i = 0; i < 100; i++)
        {
            var agent = new Agent
            {
                Position = Utility.RandomInCircle(5f),
                Rotation = Quaternion.identity,
                TargetPosition = Vector3.zero,
                StartIdleTime = Time.time,
                Index = i
            };
            _agentsData.Agents.Add(agent);
            _agentsData.ProximityGraph.Close.Add(agent, new HashSet<Edge>());
            _agentsData.ProximityGraph.TooClose.Add(agent, new HashSet<Edge>());
        }

        _agentRenderer.GatherMeshesAndMaterials();
    }
}