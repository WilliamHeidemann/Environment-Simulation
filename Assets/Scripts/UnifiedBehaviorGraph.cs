using System.Collections.Generic;
using System.Linq;
using DataStructures;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    private AgentsData _agentsData;
    [SerializeField] private float CohesionStrength;
    [SerializeField] private float AlignmentStrength;
    [SerializeField] private float SeparationStrength;

    [SerializeField] private Transform Shepherd;

    private FlockingJob _flockingJob;
    private int _count;

    private SpatialHashGrid _spatialHashGrid;

    public void Initialize(AgentsData agentsData)
    {
        _agentsData = agentsData;
        _count = agentsData.Transforms.Length;
        _spatialHashGrid = new SpatialHashGrid(4);

        _flockingJob = new FlockingJob
        {
            Transforms = agentsData.Transforms,
            Motions = agentsData.Motions,
            
            Close = new NativeArray<AgentTransform>(_count * 100, Allocator.Persistent),
            Offset = new NativeArray<int>(_count, Allocator.Persistent),
            Lengths = new NativeArray<int>(_count, Allocator.Persistent),
            CohesionStrength = CohesionStrength,
            AlignmentStrength = AlignmentStrength,
            SeparationStrength = SeparationStrength,
            DebugCentreOfFlock = new NativeArray<Vector3>(_count, Allocator.Persistent),
            DebugAlignment = new NativeArray<Vector3>(_count, Allocator.Persistent),
            DebugSeparation = new NativeArray<Vector3>(_count, Allocator.Persistent),
        };
    }

    public void Update()
    {
        _flockingJob.CohesionStrength = CohesionStrength;
        _flockingJob.AlignmentStrength = AlignmentStrength;
        _flockingJob.SeparationStrength = SeparationStrength;

        UpdateSpatialHashGrid();

        int offset = 0;
        for (int i = 0; i < _count; i++)
        {
            AgentTransform agentTransform = _agentsData.Transforms[i];
            List<AgentTransform> nearbyAgents = _spatialHashGrid.GetNearby(agentTransform.Position);
            _flockingJob.Offset[i] = offset;
            _flockingJob.Lengths[i] = nearbyAgents.Count;
            for (int j = 0; j < nearbyAgents.Count; j++)
            {
                _flockingJob.Close[j + offset] = nearbyAgents[j];
            }
            offset += nearbyAgents.Count;
        }

        _flockingJob.DeltaTime = Time.deltaTime;
        _flockingJob.Schedule(_count, 1).Complete();
    }

    private void UpdateSpatialHashGrid()
    {
        _spatialHashGrid.Clear();
        for (int i = 0; i < _agentsData.Transforms.Length; i++)
        {
            AgentTransform agentTransform = _agentsData.Transforms[i];
            _spatialHashGrid.Set(agentTransform);
        }
        _spatialHashGrid.Solidify();
    }

    private void OnDestroy()
    {
        _flockingJob.Close.Dispose();
        _flockingJob.Offset.Dispose();
        _flockingJob.Lengths.Dispose();
        
        _flockingJob.DebugCentreOfFlock.Dispose();
        _flockingJob.DebugAlignment.Dispose();
        _flockingJob.DebugSeparation.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (_agentsData == null) return;
        var zip = _agentsData.Motions.Zip(_agentsData.Transforms, (motion, agentTransform) => (motion, agentTransform));
        foreach ((AgentMotion motion, AgentTransform agentTransform) in zip)
        {
            // Debug Velocity
            Gizmos.color = Color.black;
            Gizmos.DrawLine(agentTransform.Position, agentTransform.Position + motion.Velocity);
            Gizmos.DrawCube(agentTransform.Position + motion.Velocity, Vector3.one * 0.4f);
        }

        for (int i = 0; i < _agentsData.Transforms.Length; i++)
        {
            AgentTransform agentTransform = _agentsData.Transforms[i];
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(agentTransform.Position, agentTransform.Position + _flockingJob.DebugCentreOfFlock[i]);
            Gizmos.DrawCube(agentTransform.Position + _flockingJob.DebugCentreOfFlock[i], Vector3.one * 0.4f);
            
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(agentTransform.Position, agentTransform.Position + _flockingJob.DebugAlignment[i]);
            Gizmos.DrawCube(agentTransform.Position + _flockingJob.DebugAlignment[i], Vector3.one * 0.4f);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(agentTransform.Position, agentTransform.Position + _flockingJob.DebugSeparation[i]);
            Gizmos.DrawCube(agentTransform.Position + _flockingJob.DebugSeparation[i], Vector3.one * 0.4f);
        }
    }
}