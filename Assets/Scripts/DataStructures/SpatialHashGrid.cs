using System.Collections.Generic;
using System.Linq;
using DataStructures;
using Unity.Mathematics;
using UnityEngine;

namespace DataStructures
{
    public class SpatialHashGrid
    {
        private readonly Dictionary<int2, HashSet<AgentTransform>> _grid;
        private readonly Dictionary<int2, List<AgentTransform>> _solidifiedGrid;
        private readonly int _cellSize;

        public SpatialHashGrid(int cellSize)
        {
            _cellSize = cellSize;
            _grid = new Dictionary<int2, HashSet<AgentTransform>>();
            _solidifiedGrid = new Dictionary<int2, List<AgentTransform>>();
        }

        public void Clear()
        {
            _grid.Clear();
            _solidifiedGrid.Clear();
        }

        public void Set(AgentTransform agentTransform)
        {
            int2 cell = GetCell(agentTransform.Position);

            if (!_grid.ContainsKey(cell))
            {
                _grid[cell] = new HashSet<AgentTransform>();
            }

            _grid[cell].Add(agentTransform);
        }

        public void Solidify()
        {
            foreach ((int2 cell, HashSet<AgentTransform> agents) in _grid)
            {
                if (agents.Count == 0) continue;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        int2 neighborCell = cell + new int2(x, y);

                        if (!_solidifiedGrid.ContainsKey(neighborCell))
                        {
                            _solidifiedGrid.Add(neighborCell, new List<AgentTransform>());
                        }

                        _solidifiedGrid[neighborCell].AddRange(agents);
                    }
                }
            }
        }

        public List<AgentTransform> GetNearby(float3 position)
        {
            int2 cell = GetCell(position);
            if (!_solidifiedGrid.TryGetValue(cell, out var agents))
            {
                return new List<AgentTransform>();
            }

            return agents;
        }

        public Vector3[] GetGridLines() =>
            _grid.Keys.Where(key => _grid[key].Count > 0)
                .SelectMany(GetCellOutline)
                .Select(f3 => new Vector3(f3.x, f3.y, f3.z))
                .ToArray();

        private IEnumerable<float3> GetCellOutline(int2 cell)
        {
            yield return new float3(cell.x * _cellSize, 0.1f, cell.y * _cellSize);
            yield return new float3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize);
            yield return new float3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize);
            yield return new float3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new float3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new float3(cell.x * _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new float3(cell.x * _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new float3(cell.x * _cellSize, 0.1f, cell.y * _cellSize);
        }

        private int2 GetCell(float3 position)
        {
            int x = Mathf.FloorToInt(position.x / _cellSize);
            int y = Mathf.FloorToInt(position.z / _cellSize);
            return new int2(x, y);
        }
    }
}