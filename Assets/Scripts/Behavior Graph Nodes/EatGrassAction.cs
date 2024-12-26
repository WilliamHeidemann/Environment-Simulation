using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Object = UnityEngine.Object;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Eat Grass", story: "Eat [Grass]", category: "Action", id: "f4fadf06b512ac354da63c25089848c4")]
public partial class EatGrassAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Grass;

    protected override Status OnStart()
    {
        if (Grass.Value == null)
        {
            return Status.Failure;
        }
        
        Object.Destroy(Grass.Value);
        
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

