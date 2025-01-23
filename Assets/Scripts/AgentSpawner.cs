using System;
using System.Collections.Generic;
using DataStructures;
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
        _agentsData.Transforms = new List<AgentTransform>();
        _agentsData.Motions = new List<AgentMotion>();
        Random.InitState(0);
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 position = Utility.RandomInCircle(_spawnRadius);
            Quaternion rotation = Quaternion.LookRotation(Utility.RandomOnCircle());
            
            var agentTransform = new AgentTransform
            {
                Position = position,
                Rotation = rotation,
            };
            
            var agentMotion = new AgentMotion
            {
                Speed = 0f,
                Velocity = Vector3.zero
            };
            
            _agentsData.Transforms.Add(agentTransform);
            _agentsData.Motions.Add(agentMotion);
        }

        _agentRenderer.GatherMeshesAndMaterials();
        _agentRenderer.SetupShader();
    }
}