using System.Collections.Generic;
using UnityEngine;
using UtilityToolkit.Runtime;

public class SheepSpawner : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    
    private void Start()
    {
        _agentsData.Agents = new List<Agent>();
        for (int i = 0; i < 3; i++)
        {
            _agentsData.Agents.Add(new Agent()
            {
                Position = Utility.RandomOnCircle(20f),
                Rotation = Quaternion.identity,
                TargetPosition = Option<Vector3>.None,
                StartIdleTime = Time.time
            });            
        }
    }
}