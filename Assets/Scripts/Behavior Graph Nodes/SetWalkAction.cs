using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Walk", story: "[Set] [Walk]", category: "Action", id: "a91b1df97893195c9283aed5a1ac7d72")]
public partial class SetWalkAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Set;
    [SerializeReference] public BlackboardVariable<Walk> Walk;

    protected override Status OnStart()
    {
        if (Walk.Value == null)
        {
            return Status.Failure;
        }
        
        Walk.Value.ShouldWalk = Set.Value;
        
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

