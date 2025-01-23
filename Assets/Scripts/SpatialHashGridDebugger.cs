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
        _spatialHashGrid.Clear();
        foreach (var agent in _agentsData.Transforms)
        {
            _spatialHashGrid.Set(agent);
        }
        _spatialHashGrid.Solidify();
    }

    private void OnDrawGizmos()
    {
        if (_spatialHashGrid == null)
        {
            return;
        }

        if (_agentsData.Transforms == null)
        {
            return;
        }
        
        Gizmos.color = Color.black;
        var lines = _spatialHashGrid.GetGridLines().ToArray();
        Gizmos.DrawLineList(lines);
        
        Gizmos.color = Color.red;
        foreach (var agent in _agentsData.Transforms)
        {
            var nearbyAgents = _spatialHashGrid.GetNearby(agent.Position);
            foreach (var nearbyAgent in nearbyAgents)
            {
                Gizmos.DrawLine(agent.Position, nearbyAgent.Position);
            }
        }
    }
}