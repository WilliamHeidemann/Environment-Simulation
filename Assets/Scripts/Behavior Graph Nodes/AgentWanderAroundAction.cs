using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Agent wander around", story: "[Agent] wander around", category: "Action", id: "bf9cc984f677fdd34c229966922449f2")]
public partial class AgentWanderAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<Sheep> Agent;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            return Status.Failure;
        }
        
        Agent.Value.StartWanderingTask();
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Agent.Value.IsRunningTask ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

