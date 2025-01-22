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

    private NativeArray<Vector3> _agentPositions;
    private NativeArray<Vector3> _agentForwards;
    private FlockingJob2 _flockingJob;

    private SpatialHashGrid _spatialHashGrid;

    private void Start()
    {
        int count = AgentsData.Agents.Count;
        _agentPositions = new NativeArray<Vector3>(count, Allocator.Persistent);
        _agentForwards = new NativeArray<Vector3>(count, Allocator.Persistent);
        _spatialHashGrid = new SpatialHashGrid(4);

        var accelerations = new NativeArray<Vector3>(count, Allocator.Persistent);
        _flockingJob = new FlockingJob2
        {
            Acceleration = accelerations,
            Speed = new NativeArray<float>(count, Allocator.Persistent),
            Close = new NativeArray<Edge>(count * 100, Allocator.Persistent),
            Offset = new NativeArray<int>(count, Allocator.Persistent),
            Lengths = new NativeArray<int>(count, Allocator.Persistent),
            CohesionStrength = CohesionStrength,
            AlignmentStrength = AlignmentStrength,
            SeparationStrength = SeparationStrength
        };
    }

    public void Update()
    {
        int agentCount = AgentsData.Agents.Count;
        
        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            _agentPositions[i] = agent.Position;
            _agentForwards[i] = agent.Rotation * Vector3.forward;
            _spatialHashGrid.Set(agent);
        }

        int offset = 0;
        for (int i = 0; i < agentCount; i++)
        {
            Agent agent = AgentsData.Agents[i];
            var nearby = _spatialHashGrid.GetNearby(agent)
                .Select(agent2 => new Edge
                {
                    SquareDistance = Vector3.SqrMagnitude(agent.Position - agent2.Position),
                    SeparationVector = agent.Position - agent2.Position,
                    EndPosition = agent2.Position,
                    EndForward = agent2.Rotation * Vector3.forward,
                }).ToArray();
            
            _flockingJob.Offset[i] = offset;
            for (int j = offset; j < nearby.Length; j++)
            {
                _flockingJob.Close[j] = nearby[j];
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
            agent.Speed = _flockingJob.Speed[i];
        }

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
        _agentPositions.Dispose();
        _agentForwards.Dispose();
        _flockingJob.Acceleration.Dispose();
        _flockingJob.Speed.Dispose();
        _flockingJob.Close.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (AgentsData.Agents == null) return;
        foreach (Agent agent in AgentsData.Agents)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(agent.Position, agent.Velocity);
            Gizmos.DrawCube(agent.Velocity, Vector3.one * 0.4f);
        }
        // foreach (Agent agent in AgentsData.Agents)
        // {
        //     Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        //     Gizmos.DrawWireSphere(agent.Position, 5f);
        // }
    }
}