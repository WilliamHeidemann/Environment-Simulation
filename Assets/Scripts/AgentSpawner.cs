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
        Random.InitState(0);
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 position = Utility.RandomInCircle(_spawnRadius);
            Quaternion rotation = Quaternion.LookRotation(Utility.RandomOnCircle());
            
            var agent = new Agent
            {
                Position = position,
                Rotation = rotation,
                Velocity = position,
            };
            
            _agentsData.Agents.Add(agent);
        }

        _agentRenderer.GatherMeshesAndMaterials();
        _agentRenderer.SetupShader();
    }
}