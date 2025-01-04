using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityToolkit.Runtime;
using Random = UnityEngine.Random;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private AgentRenderer _agentRenderer;
    [SerializeField] private int _spawnCount;
    [SerializeField] private int _spawnRadius;

    private void Start()
    {
        _agentsData.Agents = new List<Agent>();
        _agentsData.ProximityGraph = new ProximityGraph();
        Random.InitState(0);
        for (int i = 0; i < _spawnCount; i++)
        {
            Behavior behavior = Behavior.Flocking; //Random.value > 0.9f ? Behavior.Wandering : Behavior.Flocking;
            Vector3 position = Utility.RandomInCircle(_spawnRadius);
            Quaternion rotation = Quaternion.LookRotation(Utility.RandomOnCircle());
            Vector3 scale = behavior == Behavior.Wandering ? Vector3.one * 2 : Vector3.one;
            
            var agent = new Agent
            {
                Position = position,
                Rotation = rotation,
                Scale = scale,
                TargetPosition = position,
                Behavior = behavior,
                Index = i
            };
            
            _agentsData.Agents.Add(agent);
            _agentsData.ProximityGraph.Close.Add(agent, Array.Empty<Edge>());
            _agentsData.ProximityGraph.TooClose.Add(agent, Array.Empty<Edge>());
        }

        _agentRenderer.GatherMeshesAndMaterials();
    }
}