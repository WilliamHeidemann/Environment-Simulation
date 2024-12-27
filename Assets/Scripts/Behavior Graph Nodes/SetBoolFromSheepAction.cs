using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set bool from sheep", story: "Set [bool] from [sheep]", category: "Action", id: "c9980b32ffa5f98cd96159da93b4de0e")]
public partial class SetBoolFromSheepAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Bool;
    [SerializeReference] public BlackboardVariable<Sheep> Sheep;
    protected override Status OnStart()
    {
        if (Sheep.Value == null)
        {
            return Status.Failure;
        }
        
        Bool.Value = Sheep.Value.IsHungry;
        
        return Status.Success;
    }
}

