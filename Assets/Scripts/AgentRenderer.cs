using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentRenderer : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private GameObject _agentPrefab;

    private readonly List<SubMesh> _subMeshes = new();
    private readonly Dictionary<SubMesh, Matrix4x4[]> _instanceMatrices = new();

    private struct SubMesh
    {
        public Mesh Mesh;
        public Material[] Materials;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    private void Update()
    {
        GenerateInstanceMatrices();
        Render();
    }

    public void GatherMeshesAndMaterials()
    {
        var meshFilters = _agentPrefab.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            var subMesh = new SubMesh
            {
                Mesh = meshFilter.sharedMesh,
                Materials = meshRenderer.sharedMaterials,
                Position = meshFilter.transform.position,
                Rotation = meshFilter.transform.rotation,
                Scale = meshFilter.transform.lossyScale
            };
            
            _subMeshes.Add(subMesh);
            _instanceMatrices[subMesh] = new Matrix4x4[_agentsData.Agents.Count];
        }
    }

    private void GenerateInstanceMatrices()
    {
        foreach (SubMesh subMesh in _subMeshes)
        {
            Matrix4x4[] matrices = _instanceMatrices[subMesh];
            for (var i = 0; i < matrices.Length; i++)
            {
                Vector3 rotatedPosition = _agentsData.Agents[i].Rotation * subMesh.Position;
                Vector3 position = _agentsData.Agents[i].Position + rotatedPosition;
                
                Quaternion rotation = _agentsData.Agents[i].Rotation * subMesh.Rotation;

                Vector3 scale = subMesh.Scale;
                scale.Scale(_agentsData.Agents[i].Scale);
                matrices[i] = Matrix4x4.TRS(position, rotation, scale);
            }
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
                    instanceData: _instanceMatrices[subMesh],
                    instanceCount: _instanceMatrices[subMesh].Length
                );
            }
        }
    }
}