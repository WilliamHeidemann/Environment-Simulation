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

    private Mesh _combinedMesh;
    private RenderParams _materialRenderParams;
    private Matrix4x4[] _agentTransforms;
    private GraphicsBuffer _commandBuffer;
    private GraphicsBuffer _transformBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
    [SerializeField] private Material _shader;
    private const int CommandCount = 1;

    private void Start()
    {
        _agentTransforms = new Matrix4x4[_agentsData.Agents.Count];
        
        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];

        _commandData[0].indexCountPerInstance = _combinedMesh.GetIndexCount(0);
        _commandData[0].instanceCount = (uint)_agentTransforms.Length;
        _commandData[0].startIndex = _combinedMesh.GetIndexStart(0);
        _commandData[0].baseVertexIndex = _combinedMesh.GetBaseVertex(0);
        _commandData[0].startInstance = 0;
        
        _commandBuffer.SetData(_commandData);
        
        _transformBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _agentTransforms.Length, 64);
        _transformBuffer.SetData(_agentTransforms);
        
        _shader.SetBuffer("transformBuffer", _transformBuffer);
    }

    private void Update()
    {
        GenerateInstanceMatrices();
        Render();
    }

    public void GatherMeshesAndMaterials()
    {
        // var meshFilters = _agentPrefab.GetComponentsInChildren<MeshFilter>();
        // foreach (MeshFilter meshFilter in meshFilters)
        // {
        //     var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
        //
        //     var subMesh = new SubMesh
        //     {
        //         Mesh = meshFilter.sharedMesh,
        //         Materials = meshRenderer.sharedMaterials,
        //         Position = meshFilter.transform.position,
        //         Rotation = meshFilter.transform.rotation,
        //         Scale = meshFilter.transform.lossyScale
        //     };
        //
        //     _subMeshes.Add(subMesh);
        //     _instanceMatrices[subMesh] = new Matrix4x4[_agentsData.Agents.Count];
        // }

        // Combine meshes
        var meshFilters = _agentPrefab.GetComponentsInChildren<MeshFilter>();
        var combineInstances = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        _combinedMesh.CombineMeshes(combineInstances);

        Material material = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;
        _materialRenderParams = new RenderParams(material)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000)
        };
    }

    private void GenerateInstanceMatrices()
    {
        // foreach (SubMesh subMesh in _subMeshes)
        // {
        //     Matrix4x4[] matrices = _instanceMatrices[subMesh];
        //     for (var i = 0; i < matrices.Length; i++)
        //     {
        //         Vector3 rotatedPosition = _agentsData.Agents[i].Rotation * subMesh.Position;
        //         Vector3 position = _agentsData.Agents[i].Position + rotatedPosition;
        //
        //         Quaternion rotation = _agentsData.Agents[i].Rotation * subMesh.Rotation;
        //
        //         Vector3 scale = subMesh.Scale;
        //         scale.Scale(_agentsData.Agents[i].Scale);
        //         matrices[i] = Matrix4x4.TRS(position, rotation, scale);
        //     }
        // }
        
        for (int i = 0; i < _agentTransforms.Length; i++)
        {
            Vector3 position = _agentsData.Agents[i].Position;
            Quaternion rotation = _agentsData.Agents[i].Rotation;
            // Vector3 scale = _agentsData.Agents[i].Scale;
            _agentTransforms[i] = Matrix4x4.TRS(position, rotation, Vector3.one);
        }
    }


    private void Render()
    {
        // foreach (SubMesh subMesh in _subMeshes)
        // {
        //     for (var i = 0; i < subMesh.Materials.Length; i++)
        //     {
        //         Graphics.RenderMeshInstanced(
        //             rparams: new RenderParams(subMesh.Materials[i]),
        //             mesh: subMesh.Mesh,
        //             submeshIndex: i,
        //             instanceData: _instanceMatrices[subMesh],
        //             instanceCount: _instanceMatrices[subMesh].Length
        //         );
        //     }
        // }
        
        Graphics.RenderMeshIndirect(
            rparams: _materialRenderParams,
            mesh: _combinedMesh,
            commandBuffer: _commandBuffer,
            commandCount: CommandCount);

    }

    private void OnDestroy()
    {
        _commandBuffer.Release();
        _transformBuffer.Release();
    }
}