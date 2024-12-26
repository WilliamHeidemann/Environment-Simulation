using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Target in Walk", story: "Set [Target] in [Walk]", category: "Action", id: "c9d49f80a9cf16b23567d4a5ffc11a20")]
public partial class SetTargetInWalkAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<Walk> Walk;

    protected override Status OnStart()
    {
        if (Target.Value == null)
        {
            return Status.Failure;
        }
        
        if (Walk.Value == null)
        {
            return Status.Failure;
        }
        
        Walk.Value.SetTarget(Target.Value.transform);
        
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

