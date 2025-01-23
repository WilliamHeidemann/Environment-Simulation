using System;
using System.Linq;
using DataStructures;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AgentTranslator : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    
    private NativeArray<Vector3> _velocities;
    private NativeArray<float> _agentSpeeds;
    private NativeArray<Vector3> _agentPositions;
    private NativeArray<Quaternion> _agentRotations;
    
    // [SerializeField] private HexagonalSpatialGridDebugger _hexagonalSpatialGridDebugger;

    private void Start()
    {
        var agents = _agentsData.Transforms;
        _velocities = new NativeArray<Vector3>(agents.Count, Allocator.Persistent);
        _agentSpeeds = new NativeArray<float>(agents.Count, Allocator.Persistent);
        _agentPositions = new NativeArray<Vector3>(agents.Count, Allocator.Persistent);
        _agentRotations = new NativeArray<Quaternion>(agents.Count, Allocator.Persistent);
    }

    private void Update()
    {
        var transforms = _agentsData.Transforms;
        var motions = _agentsData.Motions;

        for (int i = 0; i < transforms.Count; i++)
        {
            _agentPositions[i] = transforms[i].Position;
            _agentRotations[i] = transforms[i].Rotation;
            _velocities[i] = motions[i].Velocity;
            _agentSpeeds[i] = motions[i].Speed;
        }

        var job = new TranslateJob
        {
            Velocities = _velocities,
            AgentSpeeds = _agentSpeeds,
            AgentPositions = _agentPositions,
            AgentRotations = _agentRotations,
            DeltaTime = Time.deltaTime
        };

        job.Schedule(transforms.Count, 1).Complete();

        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i] = new AgentTransform
            {
                Position = _agentPositions[i],
                Rotation = _agentRotations[i]
            };
        }
    }

    private void OnDestroy()
    {
        _velocities.Dispose();
        _agentSpeeds.Dispose();
        _agentPositions.Dispose();
        _agentRotations.Dispose();
    }
}