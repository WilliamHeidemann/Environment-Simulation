using System.Collections.Generic;
using System.Linq;
using DataStructures;
using UnityEngine;

namespace DataStructures
{
    public class SpatialHashGrid
    {
        private readonly Dictionary<Vector2Int, HashSet<AgentTransform>> _grid;
        private readonly Dictionary<Vector2Int, List<AgentTransform>> _solidifiedGrid;
        private readonly int _cellSize;

        public SpatialHashGrid(int cellSize)
        {
            _cellSize = cellSize;
            _grid = new Dictionary<Vector2Int, HashSet<AgentTransform>>();
            _solidifiedGrid = new Dictionary<Vector2Int, List<AgentTransform>>();
        }

        public void Clear()
        {
            _grid.Clear();
            _solidifiedGrid.Clear();
        }

        public void Set(AgentTransform agentTransform)
        {
            Vector2Int cell = GetCell(agentTransform.Position);

            if (!_grid.ContainsKey(cell))
            {
                _grid[cell] = new HashSet<AgentTransform>();
            }

            _grid[cell].Add(agentTransform);
        }

        public void Solidify()
        {
            foreach ((Vector2Int cell, HashSet<AgentTransform> agents) in _grid)
            {
                if (agents.Count == 0) continue;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        Vector2Int neighborCell = cell + new Vector2Int(x, y);

                        if (!_solidifiedGrid.ContainsKey(neighborCell))
                        {
                            _solidifiedGrid.Add(neighborCell, new List<AgentTransform>());
                        }

                        _solidifiedGrid[neighborCell].AddRange(agents);
                    }
                }
            }
        }

        public List<AgentTransform> GetNearby(Vector3 position)
        {
            Vector2Int cell = GetCell(position);
            if (!_solidifiedGrid.TryGetValue(cell, out var agents))
            {
                return new List<AgentTransform>();
            }

            return agents;
        }

        public IEnumerable<Vector3> GetGridLines() =>
            _grid.Keys.Where(key => _grid[key].Count > 0)
                .SelectMany(GetCellOutline);

        private IEnumerable<Vector3> GetCellOutline(Vector2Int cell)
        {
            yield return new Vector3(cell.x * _cellSize, 0.1f, cell.y * _cellSize);
            yield return new Vector3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize);
            yield return new Vector3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize);
            yield return new Vector3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new Vector3(cell.x * _cellSize + _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new Vector3(cell.x * _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new Vector3(cell.x * _cellSize, 0.1f, cell.y * _cellSize + _cellSize);
            yield return new Vector3(cell.x * _cellSize, 0.1f, cell.y * _cellSize);
        }

        private Vector2Int GetCell(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / _cellSize);
            int y = Mathf.FloorToInt(position.z / _cellSize);
            return new Vector2Int(x, y);
        }
    }
}