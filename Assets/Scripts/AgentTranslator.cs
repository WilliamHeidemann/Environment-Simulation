using System;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AgentTranslator : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    
    private NativeArray<Vector3> _targetPositions;
    private NativeArray<float> _agentSpeeds;
    private NativeArray<Vector3> _agentPositions;
    private NativeArray<Quaternion> _agentRotations;
    
    // [SerializeField] private HexagonalSpatialGridDebugger _hexagonalSpatialGridDebugger;

    private void Start()
    {
        var agents = _agentsData.Agents;
        _targetPositions = new NativeArray<Vector3>(agents.Count, Allocator.Persistent);
        _agentSpeeds = new NativeArray<float>(agents.Count, Allocator.Persistent);
        _agentPositions = new NativeArray<Vector3>(agents.Count, Allocator.Persistent);
        _agentRotations = new NativeArray<Quaternion>(agents.Count, Allocator.Persistent);
    }

    private void Update()
    {
        var agents = _agentsData.Agents;

        for (int i = 0; i < agents.Count; i++)
        {
            _targetPositions[i] = agents[i].TargetPosition;
            _agentSpeeds[i] = agents[i].Speed;
            _agentPositions[i] = agents[i].Position;
            _agentRotations[i] = agents[i].Rotation;
        }

        var job = new TranslateJob
        {
            TargetPositions = _targetPositions,
            AgentSpeeds = _agentSpeeds,
            AgentPositions = _agentPositions,
            AgentRotations = _agentRotations,
            DeltaTime = Time.deltaTime
        };

        job.Schedule(agents.Count, 1).Complete();

        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].Position = _agentPositions[i];
            agents[i].Rotation = _agentRotations[i];
        }
        
        // _hexagonalSpatialGridDebugger.UpdateHexGrid(agents);
    }

    private void OnDestroy()
    {
        _targetPositions.Dispose();
        _agentSpeeds.Dispose();
        _agentPositions.Dispose();
        _agentRotations.Dispose();
    }
}