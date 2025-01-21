using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataStructures
{
    public interface ISpatialHashGrid
    {
        public void Set(Agent agent);
        public IEnumerable<Agent> GetNearby(Agent agent);
    }

    public class SpatialHashGrid : ISpatialHashGrid
    {
        private readonly Dictionary<Vector2Int, HashSet<Agent>> _grid;
        private readonly Dictionary<Agent, Vector2Int> _agentToCell;
        private int _cellSize;

        public SpatialHashGrid(int cellSize)
        {
            _cellSize = cellSize;
            _grid = new Dictionary<Vector2Int, HashSet<Agent>>();
            _agentToCell = new Dictionary<Agent, Vector2Int>();
        }

        public void Set(Agent agent)
        {
            if (_agentToCell.TryGetValue(agent, out var previousCell))
            {
                _grid[previousCell].Remove(agent);
            }

            Vector2Int cell = GetCell(agent.Position);
            _agentToCell[agent] = cell;
            
            if (!_grid.ContainsKey(cell))
            {
                _grid[cell] = new HashSet<Agent>();
            }

            _grid[cell].Add(agent);
        }

        public IEnumerable<Agent> GetNearby(Agent agent)
        {
            Vector2Int cell = GetCell(agent.Position);
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector2Int neighborCell = cell + new Vector2Int(x, y);

                    if (!_grid.TryGetValue(neighborCell, out var neighborAgents))
                    {
                        continue;
                    }

                    foreach (Agent neighbor in neighborAgents)
                    {
                        yield return neighbor;
                    }
                }
            }
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