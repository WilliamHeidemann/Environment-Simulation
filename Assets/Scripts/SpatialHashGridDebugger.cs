using System;
using System.Linq;
using DataStructures;
using UnityEngine;

public class SpatialHashGridDebugger : MonoBehaviour
{
    private SpatialHashGrid _spatialHashGrid;
    [SerializeField] private int _cellSize;

    [SerializeField] private AgentsData _agentsData;

    private void Start()
    {
        _spatialHashGrid = new SpatialHashGrid(_cellSize);
    }

    private void Update()
    {
        foreach (var agent in _agentsData.Agents)
        {
            _spatialHashGrid.Set(agent);
        }
    }

    private void OnDrawGizmos()
    {
        if (_spatialHashGrid == null)
        {
            return;
        }

        if (_agentsData.Agents == null)
        {
            return;
        }
        
        Gizmos.color = Color.black;
        var lines = _spatialHashGrid.GetGridLines().ToArray();
        Gizmos.DrawLineList(lines);
        
        Gizmos.color = Color.red;
        foreach (var agent in _agentsData.Agents)
        {
            var nearbyAgents = _spatialHashGrid.GetNearby(agent);
            foreach (var nearbyAgent in nearbyAgents)
            {
                Gizmos.DrawLine(agent.Position, nearbyAgent.Position);
            }
        }
    }
}