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
        _translateJob.DeltaTime = Time.deltaTime;
        _translateJob.Schedule(_count, 1).Complete();
    }
}