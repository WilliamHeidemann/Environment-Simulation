using System;
using System.Linq;
using DataStructures;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AgentTranslator : MonoBehaviour
{
    [SerializeField] private AgentsData _agentsData;
    [SerializeField] private Terrain _terrain;

    private TranslateJob _translateJob;
    private int _count;

    public void Initialize(AgentsData agentsData)
    {
        _translateJob = new TranslateJob
        {
            AgentTransforms = agentsData.Transforms,
            AgentMotions = agentsData.Motions
        };

        _count = agentsData.Transforms.Length;
    }

    private void Update()
    {
        // for (int i = 0; i < _agentsData.Transforms.Length; i++)
        // {
        //     AgentTransform agentTransform = _agentsData.Transforms[i];
        //     AgentMotion agentMotion = _agentsData.Motions[i];
        //     var velocity = agentMotion.Velocity.normalized * (agentMotion.Speed * Time.deltaTime);
        //     float height = _terrain.SampleHeight(agentTransform.Position + velocity);
        //     agentMotion.Velocity = new Vector3(agentMotion.Velocity.x, height - agentTransform.Position.y, agentMotion.Velocity.z); 
        //     _agentsData.Motions[i] = agentMotion;
        // }

        _translateJob.DeltaTime = Time.deltaTime;
        _translateJob.Schedule(_count, 1).Complete();

        // var terrainData = _terrain.terrainData;
        //
        // for (int i = 0; i < _agentsData.Transforms.Length; i++)
        // {
        //     AgentTransform agentTransform = _agentsData.Transforms[i];
        //     float height = _terrain.SampleHeight(agentTransform.Position);
        //     agentTransform.Position = new Vector3(agentTransform.Position.x, height, agentTransform.Position.z);
        //
        //     AgentMotion agentMotion = _agentsData.Motions[i];
        //     var velocity = agentMotion.Velocity;
        //     var terrainPosition = agentTransform.Position - _terrain.transform.position;
        //     var u = terrainPosition.x / terrainData.size.x;
        //     var v = terrainPosition.z / terrainData.size.z;
        //     var terrainNormal = terrainData.GetInterpolatedNormal(u, v);
        //     var projection = Vector3.ProjectOnPlane(velocity, terrainNormal);
        //     var rotation = Quaternion.LookRotation(projection, terrainNormal);
        //     agentTransform.Rotation = rotation;
        //     _agentsData.Transforms[i] = agentTransform;
        // }
    }
}