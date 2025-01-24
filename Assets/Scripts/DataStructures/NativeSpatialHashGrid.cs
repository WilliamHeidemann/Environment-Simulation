using System;
using Unity.Collections;
using UnityEngine;

namespace DataStructures
{
    public struct NativeSpatialHashGrid : IDisposable
    {
        private NativeParallelMultiHashMap<Vector2Int, AgentTransform> _grid;
        private const int CellSize = 4;

        public NativeSpatialHashGrid(int initialCapacity, Allocator allocator)
        {
            _grid = new NativeParallelMultiHashMap<Vector2Int, AgentTransform>(initialCapacity, allocator);
        }
    
        public void Clear()
        {
            _grid.Clear();
        }

        public void Set(AgentTransform transform)
        {
            Vector2Int cell = GetCell(transform.Position);
            _grid.Add(cell, transform);
        }
    
        public void QueryNeighbors(Vector3 position, ref NativeList<AgentTransform> transforms)
        {
            Vector2Int centerCell = GetCell(position);
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    var cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                    QuerySingleCell(cell, ref transforms);
                }
            }
        }

        private void QuerySingleCell(Vector2Int cell, ref NativeList<AgentTransform> neighbors)
        {
            if (!_grid.ContainsKey(cell))
            {
                return;
            }

            foreach (AgentTransform transform in _grid.GetValuesForKey(cell))
            {
                neighbors.Add(transform);
            }
        }

        private static Vector2Int GetCell(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / CellSize);
            int y = Mathf.FloorToInt(position.z / CellSize);
            return new Vector2Int(x, y);
        }

        public void Dispose()
        {
            _grid.Dispose();
        }
    }
}