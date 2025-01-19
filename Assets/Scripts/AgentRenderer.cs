using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentRenderer : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private GameObject _agentPrefab;

    private Mesh _combinedMesh;
    private RenderParams _materialRenderParams;
    private Matrix4x4[] _agentTransforms;
    private GraphicsBuffer _commandBuffer;
    private GraphicsBuffer _transformBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
    [SerializeField] private Material _shader;
    private static readonly int TransformBuffer = Shader.PropertyToID("transform_buffer");
    private const int CommandCount = 1;

    private void Update()
    {
        GenerateInstanceMatrices();
        Render();
    }

    public void GatherMeshesAndMaterials()
    {
        // Combine meshes
        var meshFilters = _agentPrefab.GetComponentsInChildren<MeshFilter>();
        var combineInstances = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        _combinedMesh = new Mesh();
        _combinedMesh.CombineMeshes(combineInstances);

        _materialRenderParams = new RenderParams(_shader)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000)
        };
    }

    public void SetupShader()
    {
        _agentTransforms = new Matrix4x4[_agentsData.Agents.Count];
        
        _commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount,
            GraphicsBuffer.IndirectDrawIndexedArgs.size);
        _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];

        _commandData[0].indexCountPerInstance = _combinedMesh.GetIndexCount(0);
        _commandData[0].instanceCount = (uint)_agentTransforms.Length;
        _commandData[0].startIndex = _combinedMesh.GetIndexStart(0);
        _commandData[0].baseVertexIndex = _combinedMesh.GetBaseVertex(0);
        _commandData[0].startInstance = 0;

        _commandBuffer.SetData(_commandData);
        
        _transformBuffer = new GraphicsBuffer(
            target: GraphicsBuffer.Target.Structured, 
            usageFlags: GraphicsBuffer.UsageFlags.None,
            count: _agentTransforms.Length, 
            stride: 64
            );
        _transformBuffer.SetData(_agentTransforms);

        _shader.SetBuffer(TransformBuffer, _transformBuffer);
    }

    private void GenerateInstanceMatrices()
    {
        for (int i = 0; i < _agentTransforms.Length; i++)
        {
            Vector3 position = _agentsData.Agents[i].Position;
            Quaternion rotation = _agentsData.Agents[i].Rotation;
            // Vector3 scale = _agentsData.Agents[i].Scale;
            _agentTransforms[i] = Matrix4x4.TRS(position, rotation, Vector3.one);
        }

        _transformBuffer.SetData(_agentTransforms);
        _shader.SetBuffer(TransformBuffer, _transformBuffer);
    }


    private void Render()
    {
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