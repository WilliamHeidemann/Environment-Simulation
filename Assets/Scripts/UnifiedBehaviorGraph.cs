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
        int count = AgentsData.Agents.Count;
        _spatialHashGrid = new SpatialHashGrid(4);

        var accelerations = new NativeArray<Vector3>(count, Allocator.Persistent);
        _flockingJob = new FlockingJob
        {
            Acceleration = accelerations,
            Speed = new NativeArray<float>(count, Allocator.Persistent),
            Positions = new NativeArray<Vector3>(count, Allocator.Persistent),
            Close = new NativeArray<Edge>(count * 100, Allocator.Persistent),
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
        int agentCount = AgentsData.Agents.Count;

        _flockingJob.CohesionStrength = CohesionStrength;
        _flockingJob.AlignmentStrength = AlignmentStrength;
        _flockingJob.SeparationStrength = SeparationStrength;

        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            _spatialHashGrid.Set(agent);
        }

        int offset = 0;
        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            
            _flockingJob.Positions[i] = agent.Position;

            var nearby = _spatialHashGrid.GetNearby(agent)
                .Select(agent2 => new Edge
                {
                    SquareDistance = Vector3.SqrMagnitude(agent.Position - agent2.Position),
                    SeparationVector = agent.Position - agent2.Position,
                    EndPosition = agent2.Position,
                    EndForward = agent2.Rotation * Vector3.forward,
                }).ToArray();

            _flockingJob.Offset[i] = offset;
            for (int j = 0; j < nearby.Length; j++)
            {
                _flockingJob.Close[j + offset] = nearby[j];
            }

            offset += nearby.Length;
            _flockingJob.Lengths[i] = nearby.Length;
        }

        _flockingJob.DeltaTime = Time.deltaTime;
        _flockingJob.Schedule(agentCount, 128).Complete();

        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            agent.Velocity += _flockingJob.Acceleration[i];
            agent.Velocity = Vector3.ClampMagnitude(agent.Velocity, 5f);
            agent.Speed = _flockingJob.Speed[i];
        }
    }

    private void AvoidShepherd()
    {
        int agentCount = AgentsData.Agents.Count;
        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            float distance = Vector3.SqrMagnitude(agent.Position - Shepherd.position);
            if (distance > 100f) continue;
            Vector3 direction = agent.Position - Shepherd.position;
            float strength = -(distance * 0.1f) + 20f;
            // Vector3 separation = agent.Position - agent.TargetPosition;
            agent.Velocity += direction * (strength * 0.01f);
            agent.Speed += strength;
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
    }

    private void OnDrawGizmos()
    {
        if (AgentsData.Agents == null) return;
        foreach (Agent agent in AgentsData.Agents)
        {
            // Debug Velocity
            Gizmos.color = Color.black;
            Gizmos.DrawLine(agent.Position, agent.Position + agent.Velocity);
            Gizmos.DrawCube(agent.Position + agent.Velocity, Vector3.one * 0.4f);
        }

        for (int i = 0; i < AgentsData.Agents.Count; i++)
        {
            Agent agent = AgentsData.Agents[i];
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(agent.Position, agent.Position + _flockingJob.DebugCentreOfFlock[i]);
            Gizmos.DrawCube(agent.Position + _flockingJob.DebugCentreOfFlock[i], Vector3.one * 0.4f);
            
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(agent.Position, agent.Position + _flockingJob.DebugAlignment[i]);
            Gizmos.DrawCube(agent.Position + _flockingJob.DebugAlignment[i], Vector3.one * 0.4f);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(agent.Position, agent.Position + _flockingJob.DebugSeparation[i]);
            Gizmos.DrawCube(agent.Position + _flockingJob.DebugSeparation[i], Vector3.one * 0.4f);
        }
    }
}