using System;
using System.Collections.Generic;
using DataStructures;
using Unity.Collections;
using UnityEngine;
using UtilityToolkit.Runtime;
using Random = UnityEngine.Random;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private int _spawnCount;
    [SerializeField] private int _spawnRadius;

    private void Start()
    {
        _agentsData.Transforms = new NativeArray<AgentTransform>(_spawnCount, Allocator.Persistent);
        _agentsData.Motions = new NativeArray<AgentMotion>(_spawnCount, Allocator.Persistent);
        Random.InitState(0);
        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 position = Utility.RandomInCircle(_spawnRadius);
            Quaternion rotation = Quaternion.LookRotation(Utility.RandomOnCircle());

            _agentsData.Transforms[i] = new AgentTransform
            {
                Position = position,
                Rotation = rotation,
            };

            _agentsData.Motions[i] = new AgentMotion
            {
                Speed = 0f,
                Velocity = Vector3.zero
            };
        }
        
        GetComponent<AgentRenderer>().Initialize(_agentsData);
        GetComponent<AgentTranslator>().Initialize(_agentsData);
        GetComponent<UnifiedBehaviorGraph>().Initialize(_agentsData);
    }
}