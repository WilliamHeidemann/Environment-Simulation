using System.Collections.Generic;
using System.Linq;
using DataStructures;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class UnifiedBehaviorGraph : MonoBehaviour
{
    [SerializeField] private AgentsData AgentsData;
    [SerializeField] private float CohesionStrength;
    [SerializeField] private float AlignmentStrength;
    [SerializeField] private float SeparationStrength;

    [SerializeField] private Transform Shepherd;

    private FlockingJob _flockingJob;

    private SpatialHashGrid _spatialHashGrid;

    private void Start()
    {
        int count = AgentsData.Transforms.Count;
        _spatialHashGrid = new SpatialHashGrid(4);

        var accelerations = new NativeArray<Vector3>(count, Allocator.Persistent);
        _flockingJob = new FlockingJob
        {
            Acceleration = accelerations,
            Speed = new NativeArray<float>(count, Allocator.Persistent),
            Positions = new NativeArray<Vector3>(count, Allocator.Persistent),
            Close = new NativeArray<AgentTransform>(count * 100, Allocator.Persistent),
            Offset = new NativeArray<int>(count, Allocator.Persistent),
            Lengths = new NativeArray<int>(count, Allocator.Persistent),
            CohesionStrength = CohesionStrength,
            AlignmentStrength = AlignmentStrength,
            SeparationStrength = SeparationStrength,
            DebugCentreOfFlock = new NativeArray<Vector3>(count, Allocator.Persistent),
            DebugAlignment = new NativeArray<Vector3>(count, Allocator.Persistent),
            DebugSeparation = new NativeArray<Vector3>(count, Allocator.Persistent),
        };
    }

    public void Update()
    {
        int agentCount = AgentsData.Transforms.Count;

        _flockingJob.CohesionStrength = CohesionStrength;
        _flockingJob.AlignmentStrength = AlignmentStrength;
        _flockingJob.SeparationStrength = SeparationStrength;

        UpdateSpatialHashGrid();

        int offset = 0;
        for (int i = 0; i < agentCount; i++)
        {
            AgentTransform agentTransform = AgentsData.Transforms[i];
            
            _flockingJob.Positions[i] = agentTransform.Position;
            List<AgentTransform> nearbyAgents = _spatialHashGrid.GetNearby(agentTransform.Position);

            _flockingJob.Offset[i] = offset;
            for (int j = 0; j < nearbyAgents.Count; j++)
            {
                _flockingJob.Close[j + offset] = nearbyAgents[j];
            }

            offset += nearbyAgents.Count;
            _flockingJob.Lengths[i] = nearbyAgents.Count;
        }

        _flockingJob.DeltaTime = Time.deltaTime;
        _flockingJob.Schedule(agentCount, 128).Complete();

        for (int i = 0; i < agentCount; i++)
        {
            Vector3 velocity = AgentsData.Motions[i].Velocity + _flockingJob.Acceleration[i];
            velocity = Vector3.ClampMagnitude(velocity, 5f);
            
            AgentsData.Motions[i] = new AgentMotion
            {
                Speed = _flockingJob.Speed[i],
                Velocity = velocity
            };
        }
    }

    private void UpdateSpatialHashGrid()
    {
        _spatialHashGrid.Clear();
        for (int i = 0; i < AgentsData.Transforms.Count; i++)
        {
            AgentTransform agentTransform = AgentsData.Transforms[i];
            _spatialHashGrid.Set(agentTransform);
        }
        _spatialHashGrid.Solidify();
    }

    private void AvoidShepherd()
    {
        int agentCount = AgentsData.Transforms.Count;
        for (int i = 0; i < agentCount; i++)
        {
            AgentTransform agentTransform = AgentsData.Transforms[i];
            float distance = Vector3.SqrMagnitude(agentTransform.Position - Shepherd.position);
            if (distance > 100f) continue;
            Vector3 direction = agentTransform.Position - Shepherd.position;
            float strength = -(distance * 0.1f) + 20f;
            // Vector3 separation = agent.Position - agent.TargetPosition;
            // agentTransform.Velocity += direction * (strength * 0.01f);
            // agentTransform.Speed += strength;
        }
    }

    private void OnDestroy()
    {
        _flockingJob.Acceleration.Dispose();
        _flockingJob.Speed.Dispose();
        _flockingJob.Positions.Dispose();
        _flockingJob.Close.Dispose();
        _flockingJob.Offset.Dispose();
        _flockingJob.Lengths.Dispose();
        
        _flockingJob.DebugCentreOfFlock.Dispose();
        _flockingJob.DebugAlignment.Dispose();
        _flockingJob.DebugSeparation.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (AgentsData.Transforms == null) return;
        var zip = AgentsData.Motions.Zip(AgentsData.Transforms, (motion, agentTransform) => (motion, agentTransform));
        foreach ((AgentMotion motion, AgentTransform agentTransform) in zip)
        {
            // Debug Velocity
            Gizmos.color = Color.black;
            Gizmos.DrawLine(agentTransform.Position, agentTransform.Position + motion.Velocity);
            Gizmos.DrawCube(agentTransform.Position + motion.Velocity, Vector3.one * 0.4f);
        }

        for (int i = 0; i < AgentsData.Transforms.Count; i++)
        {
            AgentTransform agentTransform = AgentsData.Transforms[i];
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