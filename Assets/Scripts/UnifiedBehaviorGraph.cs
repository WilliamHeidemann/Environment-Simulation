using System.Collections.Generic;
using System.Linq;
using DataStructures;
using Jobs;
using Unity.Burst;
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

    private NativeSpatialHashGrid _spatialHashGrid;

    public void Initialize(AgentsData agentsData)
    {
        _agentsData = agentsData;
        _count = agentsData.Transforms.Length;
        _spatialHashGrid = new NativeSpatialHashGrid(100, Allocator.Persistent);

        _flockingJob = new FlockingJob
        {
            Transforms = agentsData.Transforms,
            Motions = agentsData.Motions,
            
            SpatialHashGrid = _spatialHashGrid,
            
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
        
        _flockingJob.DeltaTime = Time.deltaTime;
        _flockingJob.Schedule(_count, 1).Complete();
    }

    private void UpdateSpatialHashGrid()
    {
        _spatialHashGrid.Clear();
        for (int i = 0; i < _count; i++)
        {
            _spatialHashGrid.Set(_agentsData.Transforms[i]);
        }
    }

    private void OnDestroy()
    {
        _flockingJob.DebugCentreOfFlock.Dispose();
        _flockingJob.DebugAlignment.Dispose();
        _flockingJob.DebugSeparation.Dispose();
        
        _flockingJob.SpatialHashGrid.Dispose();
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