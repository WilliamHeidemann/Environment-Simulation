using System.Collections.Generic;
using UnityEngine;
using UtilityToolkit.Runtime;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private AgentRenderer _agentRenderer;
    private void Start()
    {
        _agentsData.Agents = new List<Agent>();
        for (int i = 0; i < 10000; i++)
        {
            _agentsData.Agents.Add(new Agent()
            {
                Position = Utility.RandomInCircle(20f),
                Rotation = Quaternion.identity,
                TargetPosition = Option<Vector3>.None,
                StartIdleTime = Time.time
            });            
        }
        
        _agentRenderer.GatherMeshesAndMaterials();
    }
}