using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentRenderer : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private GameObject _agentPrefab;

    private readonly List<SubMesh> _subMeshes = new();
    private readonly Dictionary<Mesh, Matrix4x4[]> _instanceMatrices = new();

    private struct SubMesh
    {
        public Mesh Mesh;
        public Material[] Materials;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    private void Start()
    {
        GatherMeshesAndMaterials();
    }

    private void Update()
    {
        GenerateInstanceMatrices();
        Render();
    }

    private void GatherMeshesAndMaterials()
    {
        var meshFilters = _agentPrefab.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            _subMeshes.Add(new SubMesh
            {
                Mesh = meshFilter.sharedMesh,
                Materials = meshRenderer.sharedMaterials,
                Position = meshFilter.transform.position,
                Rotation = meshFilter.transform.rotation,
                Scale = meshFilter.transform.lossyScale
            });
        }
    }

    private void GenerateInstanceMatrices()
    {
        int instanceCount = _agentsData.Agents.Count;

        foreach (SubMesh subMesh in _subMeshes)
        {
            var matrices = new Matrix4x4[instanceCount];
            for (var i = 0; i < instanceCount; i++)
            {
                Vector3 position = _agentsData.Agents[i].Position + subMesh.Position;
                Quaternion rotation = _agentsData.Agents[i].Rotation * subMesh.Rotation;
                Vector3 scale = subMesh.Scale;
                matrices[i] = Matrix4x4.TRS(position, rotation, scale);
            }

            _instanceMatrices[subMesh.Mesh] = matrices;
        }
    }


    private void Render()
    {
        foreach (SubMesh subMesh in _subMeshes)
        {
            for (var i = 0; i < subMesh.Materials.Length; i++)
            {
                Graphics.RenderMeshInstanced(
                    rparams: new RenderParams(subMesh.Materials[i]),
                    mesh: subMesh.Mesh,
                    submeshIndex: i,
                    instanceData: _instanceMatrices[subMesh.Mesh],
                    instanceCount: _instanceMatrices[subMesh.Mesh].Length
                );
            }
        }
    }
}