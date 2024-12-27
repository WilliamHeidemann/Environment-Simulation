using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Sheep walk to nearest grass", story: "[Sheep] walk to nearest grass", category: "Action", id: "e83d68d3ead0f0009a5a843c085e8028")]
public partial class SheepWalkToNearestGrassAction : Action
{
    [SerializeReference] public BlackboardVariable<Sheep> Sheep;

    protected override Status OnStart()
    {
        if (Sheep.Value == null)
        {
            return Status.Failure;
        }
        
        Sheep.Value.StartWalkToGrassTask();
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Sheep.Value.IsRunningTask ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

